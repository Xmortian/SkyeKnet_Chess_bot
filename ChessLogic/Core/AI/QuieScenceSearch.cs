using System;
using System.Collections.Generic;
using System.Linq;
using ChessLogic;

namespace ChessLogic.Core.Ai
{
    public static class QuiescenceSearcher
    {
        private const int MaxQuiescenceDepth = 8;
        private const int MaxQuiescenceMovesToConsider = 18;
        private const int DeltaPruningMaterialMargin = 975; 

        public static int Quiescence(StateOfGame state, int alpha, int beta, Player perspective, int depth = 0)
        {
            if (ChessAI.StopwatchElapsed())
            {
                return Evaluator.Evaluate(state.Board, perspective);
            }

            if (depth > MaxQuiescenceDepth)
            {
                return Evaluator.Evaluate(state.Board, perspective); 
            }

            int standPat = Evaluator.Evaluate(state.Board, perspective);

            if (standPat >= beta)
            {
                return beta; // Fail-high
            }

            if (standPat + DeltaPruningMaterialMargin < alpha && depth > 0) 
            {
            }


            if (alpha < standPat)
            {
                alpha = standPat;
            }

            var legalMoves = state.AllLegalMovesFor(state.CurrentPlayer);
            var tacticalMoveEntries = new List<(Move move, int score)>();

            foreach (Move move in legalMoves)
            {
                if (IsTacticalMove(move, state.Board))
                {
                    tacticalMoveEntries.Add((move, ScoreTacticalMove(move, state.Board)));
                }
            }

            var orderedTacticalMoves = tacticalMoveEntries.OrderByDescending(e => e.score).Select(e => e.move);

            int qMovesProcessed = 0;
            foreach (Move move in orderedTacticalMoves)
            {
                if (++qMovesProcessed > MaxQuiescenceMovesToConsider)
                {
                    break;
                }

                if (ChessAI.StopwatchElapsed()) 
                {
                    return alpha;
                }

                StateOfGame nextState = new StateOfGame(state.CurrentPlayer, state.Board.Copy());
                nextState.MakeMove(move);

                int score = -Quiescence(nextState, -beta, -alpha, perspective.Opponent(), depth + 1);

                if (score >= beta)
                {
                    return beta; // Fail-high
                }

                if (score > alpha)
                {
                    alpha = score;
                }
            }
            return alpha;
        }

        private static bool IsTacticalMove(Move move, board board)
        {
            Piece attacker = board[move.FromPos];
            if (attacker == null) return false;

            Piece target = board[move.ToPos];
            bool isCapture = target != null && target.Color != attacker.Color;

            bool givesCheck = GivesCheck(move, board);

            return isCapture || givesCheck;
        }

        private static bool GivesCheck(Move move, board board)
        {
            var copy = board.Copy();
            move.Execute(copy);

            Piece movingPieceOriginal = board[move.FromPos];
            if (movingPieceOriginal == null)
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    return copy.IsInCheck(copy[move.ToPos].Color.Opponent());
                }
                Player movingPlayer = board[move.FromPos]?.Color ?? Player.None; 
                if (movingPlayer == Player.None) return false; 
                return copy.IsInCheck(movingPlayer.Opponent());
            }
            return copy.IsInCheck(movingPieceOriginal.Color.Opponent());
        }

        private static int ScoreTacticalMove(Move move, board board)
        {
            int score = 0;
            Piece attacker = board[move.FromPos];
            Piece victim = board[move.ToPos];

            if (attacker == null) return 0; 

            if (victim != null) 
            {
                
                score = 10 * Evaluator.GetPieceValue(victim.Type) - Evaluator.GetPieceValue(attacker.Type);
            }

            if (GivesCheck(move, board))
            {
                score += 50; 
            }

            if (move.Type == MoveType.PawnPromotion)
            {
                PawnPromotion promotion = (PawnPromotion)move;
                score += Evaluator.GetPieceValue(promotion.newType); 
            }
            return score;
        }
    }
}

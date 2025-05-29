using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLogic.Core.Ai
{
    public static class QuiescenceSearcher
    {
        private const int CHECKMATE_SCORE = 100000;
        private const int DRAW_SCORE = 0;

        public static int Quiescence(StateOfGame state, int alpha, int beta, Player perspective)
        {
            if (ChessAI.StopwatchElapsed())
                return Evaluator.Evaluate(state.Board, perspective);

            int standPat = Evaluator.Evaluate(state.Board, perspective);

            if (standPat >= beta)
                return beta;

            if (alpha < standPat)
                alpha = standPat;

            foreach (Move move in state.AllLegalMovesFor(state.CurrentPlayer))
            {
                if (!IsTacticalMove(move, state.Board)) continue;

                if (ChessAI.StopwatchElapsed())
                    return Evaluator.Evaluate(state.Board, perspective);

                StateOfGame nextState = new StateOfGame(state.CurrentPlayer, state.Board.Copy());
                nextState.MakeMove(move);

                int score = -Quiescence(nextState, -beta, -alpha, perspective.Opponent());

                if (score >= beta)
                    return beta;

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        private static bool IsTacticalMove(Move move, board board)
        {
            Piece attacker = board[move.FromPos];
            Piece target = board[move.ToPos];

            bool isCapture = target != null && target.Color != attacker.Color;
            bool givesCheck = GivesCheck(move, board);

            return isCapture || givesCheck;
        }

        private static bool GivesCheck(Move move, board board)
        {
            var copy = board.Copy();
            move.Execute(copy);
            Player opponent = board[move.FromPos].Color.Opponent();
            return copy.IsInCheck(opponent);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using ChessLogic;

namespace ChessLogic.Core.Ai
{
    public static class MoveOrdering
    {
        private static readonly HashSet<Position> CenterSquares = new() {
            new Position(3,3),new Position(3,4),new Position(4,3),new Position(4,4),new Position(2,3),new Position(2,4),
            new Position(5,3),new Position(5,4),new Position(3,2),new Position(4,2),new Position(3,5),new Position(4,5)
        };
        private static readonly Dictionary<int, List<Move>> KillerMoves = new();
        private static readonly Dictionary<(Position from, Position to), int> HistoryHeuristic = new();

        private const int SEEWinningCaptureBias = 1200000; 
        private const int CaptureBias = 1000000;         
        private const int PromotionBias = 950000;        
        private const int KillerMoveBonus = 80000;
        private const int CheckBonus = 60000;            
        private const int CastleBonus = 30000;
        private const int PassedPawnPushBonus = 25000;   
        private const int CenterControlBonus = 1500;     

        
        private static readonly int[] see_piece_values = {
            0, 
            Evaluator.GetPieceValue(PieceType.Pawn), Evaluator.GetPieceValue(PieceType.Knight),
            Evaluator.GetPieceValue(PieceType.Bishop), Evaluator.GetPieceValue(PieceType.Rook),
            Evaluator.GetPieceValue(PieceType.Queen), Evaluator.GetPieceValue(PieceType.King)
        };


        public static List<Move> OrderMoves(IEnumerable<Move> moves, board board, int ply = 0)
        {
            return moves
                .Select(m => new { Move = m, Score = ScoreMove(m, board, ply) })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Move)
                .ToList();
        }

        private static int ScoreMove(Move move, board board, int ply)
        {
            int score = 0;
            Piece movingPiece = board[move.FromPos];
            Piece targetPiece = board[move.ToPos];

            if (movingPiece == null) return 0;

            
            if (targetPiece != null)
            {
                int seeValue = StaticExchangeEvaluation(move.ToPos, board);
                if (seeValue > 0) 
                {
                    score = 50000 + (seeValue * 10);  
                }
                else 
                {
                    score = -30000 - (Math.Abs(seeValue) * 10);  

                    
                    if (seeValue == 0)
                    {
                        score = 10000 + (Evaluator.GetPieceValue(targetPiece.Type) * 10
                                 - Evaluator.GetPieceValue(movingPiece.Type));
                    }
                }
            }

            
            if (move.Type == MoveType.PawnPromotion)
            {
                score = 100000;  
            }

            
            if (targetPiece == null)
            {
                if (KillerMoves.TryGetValue(ply, out var killers) && killers.Any(k => SameMove(k, move)))
                {
                    score += 8000;  
                }
            }

            
            var key = (move.FromPos, move.ToPos);
            if (HistoryHeuristic.TryGetValue(key, out int historyScore))
            {
                score += historyScore * 2;  
            }

            
            if (GivesCheck(move, board))
            {
                score += (ply < 3) ? 40000 : 6000;  
            }

            
            if (move.Type == MoveType.CastleKS || move.Type == MoveType.CastleQS)
            {
                score += 5000;
            }

            
            if (movingPiece.Type == PieceType.Pawn && targetPiece == null)
            {
                if (IsPassedPawnPush(move, board))
                {
                    score += 2500;
                    
                    if ((movingPiece.Color == Player.White && move.ToPos.Row == 1) ||
                        (movingPiece.Color == Player.Black && move.ToPos.Row == 6))
                    {
                        score += 2500;
                    }
                }
            }

            
            if (CenterSquares.Contains(move.ToPos))
            {
                score += 1500;
            }

            return score;
        }

        
        
        
        private static int StaticExchangeEvaluation(Position targetSquare, board board)
        {
            if (board[targetSquare] == null) return 0;

            int gain = 0;
            int depth = 0;
            int[] values = see_piece_values;
            List<Piece> attackers = new List<Piece>();

            
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);
                    Piece p = board[pos];
                    if (p == null) continue;

                    
                    if (board.IsSquareAttacked(targetSquare, p.Color))
                    {
                        attackers.Add(p);
                    }
                }
            }

            if (attackers.Count == 0) return 0;

            
            attackers.Sort((a, b) => values[(int)a.Type].CompareTo(values[(int)b.Type]));

            while (attackers.Count > 0)
            {
                Piece smallest = attackers[0];
                attackers.RemoveAt(0);

                int value = (depth == 0)
                    ? values[(int)board[targetSquare].Type]
                    : values[(int)smallest.Type];

                gain = (depth == 0)
                    ? value
                    : Math.Max(0, value - gain);

                depth++;
            }
            return gain;
        }

        private static int SeeRecursive(Position targetSquare, Player attackerSide, board currentBoard)
        {
            Piece leastValuableAttacker = null;
            Position attackerBestPos = null;
            int minAttackerValue = int.MaxValue;

            
            foreach (Position p in currentBoard.PiecePositionsFor(attackerSide))
            {
                Piece piece = currentBoard[p];
                if (piece == null) continue;

              
                IEnumerable<Move> attackerMoves = piece.GetMoves(p, currentBoard); 
                foreach (Move potentialAttack in attackerMoves)
                {
                    if (potentialAttack.ToPos.Equals(targetSquare))
                    {
                        int val = see_piece_values[(int)piece.Type];
                        if (val < minAttackerValue)
                        {
                            minAttackerValue = val;
                            leastValuableAttacker = piece;
                            attackerBestPos = p;
                        }
                        break; 
                    }
                }
            }

            if (leastValuableAttacker == null) 
            {
                return 0;
            }

            
            board nextBoard = currentBoard.Copy();
            Piece pieceOnTarget = nextBoard[targetSquare]; 
            int capturedValue = (pieceOnTarget != null) ? see_piece_values[(int)pieceOnTarget.Type] : 0;

            nextBoard[targetSquare] = leastValuableAttacker;
            nextBoard[attackerBestPos] = null;

            
            return Math.Max(0, capturedValue - SeeRecursive(targetSquare, attackerSide.Opponent(), nextBoard));
        }


        private static bool GivesCheck(Move move, board board)
        {
            var copy = board.Copy(); move.Execute(copy);
            Player movingPlayerColor = board[move.FromPos]?.Color ?? Player.White;
            return copy.IsInCheck(movingPlayerColor.Opponent());
        }
        private static bool IsPassedPawnPush(Move move, board board)
        { /* Use robust version from previous response */
            Piece movingPiece = board[move.FromPos];
            if (movingPiece == null || movingPiece.Type != PieceType.Pawn) return false;
            Player color = movingPiece.Color; Position to = move.ToPos;
            int direction = (color == Player.White) ? -1 : 1;
            for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
            {
                int checkFile = to.Column + fileOffset;
                if (checkFile < 0 || checkFile > 7) continue;
                for (int r = to.Row + direction; r >= 0 && r < 8; r += direction)
                {
                    Piece p = board[r, checkFile];
                    if (p != null && p.Type == PieceType.Pawn && p.Color != color) return false;
                }
            }
            return true;
        }
        private static bool SameMove(Move a, Move b)
        {
            return a.FromPos.Equals(b.FromPos) && a.ToPos.Equals(b.ToPos) && a.Type == b.Type; 
        }
        public static void StoreKillerMove(Move move, int ply)
        { /* As before */
            if (!KillerMoves.ContainsKey(ply)) KillerMoves[ply] = new List<Move>();
            if (!KillerMoves[ply].Any(k => SameMove(k, move)))
            {
                if (KillerMoves[ply].Count >= 2) KillerMoves[ply].RemoveAt(0);
                KillerMoves[ply].Add(move);
            }
        }
        public static void AddHistoryBonus(Move move, int plyFromRoot)
        { /* As before */
            var key = (move.FromPos, move.ToPos); int bonus = (plyFromRoot + 1) * (plyFromRoot + 1);
            if (HistoryHeuristic.ContainsKey(key)) HistoryHeuristic[key] += bonus; else HistoryHeuristic[key] = bonus;
        }
        public static void ClearHistoryAndKillers() { KillerMoves.Clear(); HistoryHeuristic.Clear(); }
    }
}

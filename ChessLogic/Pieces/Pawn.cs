using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class Pawn : Piece
    {
        public override PieceType Type => PieceType.Pawn;
        public override Player Color { get; }

        private readonly Direction forward;
        private readonly Direction[] captureDirections;

        public Pawn(Player color)
        {
            Color = color;

            // Set movement direction
            if (color == Player.Black)
            {
                forward = Direction.South;
                captureDirections = new[] { Direction.SouthWest, Direction.SouthEast };
            }
 
            else
            {
                forward = Direction.North;
                captureDirections = new[] { Direction.NorthWest, Direction.NorthEast };
            }
        }

        public override Piece Copy()
        {
            Pawn copy = new Pawn(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private bool CanMoveTo(Position pos, board board)
        {
            return board.IsInside(pos) && board.IsEmpty(pos);
        }

        private bool CanCaptureAt(Position pos, board board)
        {
            return board.IsInside(pos) && !board.IsEmpty(pos) && board[pos].Color != Color;
        }


        private static IEnumerable<Move> PromotionMoves(Position from, Position to)
        {
            yield return new PawnPromotion(from, to, PieceType.Knight);
            yield return new PawnPromotion(from, to, PieceType.Queen);
            yield return new PawnPromotion(from,to, PieceType.Knight); 
            yield return new PawnPromotion(from, to, PieceType.Rook);
        }

        private IEnumerable<Move> ForwardMoves(Position from, board board)
        {
            Position oneStep = from + forward;
            if (CanMoveTo(oneStep, board))
            {
                if (oneStep.Row == 0 || oneStep.Row == 7)
                {
                    foreach (Move promMove in PromotionMoves(from, oneStep))
                    {
                        yield return promMove;

                    }
                }
                else
                {
                    yield return new NormalMove(from, oneStep);

                }


                Position twoSteps = oneStep + forward;
                if (!HasMoved && CanMoveTo(twoSteps, board))
                {
                    yield return new DoublePawn(from, twoSteps);
                }
            }
        }

        private IEnumerable<Move> DiagonalMoves(Position from, board board)
        {
            foreach (Direction dir in captureDirections)
            {
                Position to = from + dir;

                if (to ==board.GetPawnSkipPosition(Color.Opponent()))
                {
                    yield return new EnPassant(from, to);   
                }
                else if (CanCaptureAt(to, board))
                {
                    if (to.Row == 0 || to.Row == 7)
                    {
                        foreach (Move promMove in PromotionMoves(from, to))
                        {
                            yield return promMove;

                        }
                    }
                    else
                    {
                        yield return new NormalMove(from, to);

                    }
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, board board)
        {
            return ForwardMoves(from, board).Concat(DiagonalMoves(from, board));
        }

        public override bool CanCaptureOpponentKing(Position from, board board)
        {
            return DiagonalMoves(from, board).Any(move =>
            {
                Piece piece = board[move.ToPos];
                return piece != null && piece.Type == PieceType.King;

            });
        }
    }
}

using ChessLogic.Moves;

namespace ChessLogic
{
    public class King : Piece
    {
        public override PieceType Type => PieceType.King;
        public override Player Color { get; }

        private static readonly Direction[] dirs = new[]
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West,
            Direction.NorthEast,
            Direction.SouthEast,
            Direction.NorthWest,
            Direction.SouthWest
        };

        public King(Player color)
        {
            Color = color;
        }

        private static bool IsUnmovedRook(Position pos, board board)
        {
            if (board.IsEmpty(pos)) return false;
            Piece piece = board[pos];
            return piece.Type == PieceType.Rook && !piece.HasMoved;
        }

        private static bool AllEmpty(IEnumerable<Position> positions, board board)
        {
            return positions.All(pos => board.IsEmpty(pos));
        }

        private bool CanCastleKingSide(Position from, board board)
        {
            if (HasMoved) return false;

            Position rookPos = new Position(from.Row, 7);
            Position[] betweenPositions = new Position[] { new(from.Row, 5), new(from.Row, 6) };
            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }

        private bool CanCastleQueenSide(Position from, board board)
        {
            if (HasMoved) return false;

            Position rookPos = new Position(from.Row, 0);
            Position[] betweenPositions = new Position[] { new(from.Row, 1), new(from.Row, 2), new(from.Row, 3) };
            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }

        public override Piece Copy()
        {
            King copy = new King(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private IEnumerable<Position> SingleStepPositions(Position from, board board)
        {
            foreach (Direction dir in dirs)
            {
                Position to = from + dir;
                if (board.IsInside(0, to) && (board.IsEmpty(to) || board[to].Color != Color))
                {
                    yield return to;
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, board board, bool forAttackOnly = false)
        {
            foreach (var move in SingleStepPositions(from, board)
                .Select(to => board.IsEmpty(to)
                    ? (Move)new NormalMove(from, to)
                    : new CaptureMove(from, to)))
            {
                yield return move;
            }

            if (!forAttackOnly)
            {
                if (!board.IsInCheck(Color))
                {
                    if (CanCastleKingSide(from, board) && !SquaresUnderAttackForCastle(from, Direction.East, board))
                    {
                        yield return new Castle(MoveType.CastleKS, from);
                    }
                    if (CanCastleQueenSide(from, board) && !SquaresUnderAttackForCastle(from, Direction.West, board))
                    {
                        yield return new Castle(MoveType.CastleQS, from);
                    }
                }
            }
        }


        private bool SquaresUnderAttackForCastle(Position kingPos, Direction dir, board board)
        {
            Player player = Color;

            Position pos1 = kingPos + dir;
            Position pos2 = pos1 + dir;

            if (!board.IsInside(0, pos1) || !board.IsInside(0, pos2))
                return true; 

            return board.IsSquareAttacked(pos1, player.Opponent()) ||
                   board.IsSquareAttacked(pos2, player.Opponent());
        }


        public override bool CanCaptureOpponentKing(Position from, board board)
        {
            return SingleStepPositions(from, board).Any(to =>
            {
                Piece piece = board[to];
                return piece != null && piece.Type == PieceType.King;
            });
        }
    }
}

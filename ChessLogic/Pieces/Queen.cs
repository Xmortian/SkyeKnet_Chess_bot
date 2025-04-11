using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class Queen : Piece
    {
        public override PieceType Type => PieceType.Queen;
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

        public Queen(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Queen copy = new Queen(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        public override IEnumerable<Move> GetMoves(Position from, board board)
        {
            return MovePositionsDirs(from, board, dirs) // Fixed method name
                .Select(to => board.IsEmpty(to)         // Capitalized "Select"
                    ? (Move)new NormalMove(from, to)
                    : new CaptureMove(from, to));
        }
    }
}
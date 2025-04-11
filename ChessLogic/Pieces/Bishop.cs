using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class Bishop : Piece
    {
        public override PieceType Type => PieceType.Bishop;
        public override Player Color { get; }

        private static readonly Direction[] dirs = new[]
        {
            Direction.NorthWest,
            Direction.NorthEast,
            Direction.SouthWest,
            Direction.SouthEast
        };

        public Bishop(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Bishop copy = new Bishop(Color);
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
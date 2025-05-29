using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class Rook : Piece
    {
        public override PieceType Type => PieceType.Rook;
        public override Player Color { get; }

        private static readonly Direction[] dirs = new[]
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West
        };

        public Rook(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Rook copy = new Rook(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        public override IEnumerable<Move> GetMoves(Position from, board board, bool forAttackOnly = false)
        {
            return MovePositionsDirs(from, board, dirs) // Fixed method name
                .Select(to => board.IsEmpty(to)         // Capitalized "Select"
                    ? (Move)new NormalMove(from, to)
                    : new CaptureMove(from, to));
        }
    }
}
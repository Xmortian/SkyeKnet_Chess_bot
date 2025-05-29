using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class Knight : Piece
    {
        public override PieceType Type => PieceType.Knight;
        public override Player Color { get; }

        public Knight(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Knight copy = new Knight(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private static IEnumerable<Position> PotentialPositions(Position from)
        {
            foreach (Direction vDir in new[] { Direction.North, Direction.South })
            {
                foreach (Direction hDir in new[] { Direction.East, Direction.West })
                {
                    yield return from + 2 * vDir + hDir;
                    yield return from + 2 * hDir + vDir;
                }
            }
        }

        private IEnumerable<Position> MovePositions(Position from, board board)
        {
            return PotentialPositions(from)
                .Where(pos => board.IsInside(0, pos) &&
                    (board.IsEmpty(pos) || board[pos].Color != Color));
        }

        public override IEnumerable<Move> GetMoves(Position from, board board, bool forAttackOnly = false)
        {
            return MovePositions(from, board)
                .Select(to => board.IsEmpty(to)
                    ? (Move)new NormalMove(from, to)
                    : new CaptureMove(from, to));
        }
    }
}
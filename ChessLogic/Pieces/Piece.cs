using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    // Abstract class representing a chess piece
    public abstract class Piece
    {
        // Abstract property to get the type of the piece (e.g., Pawn, Rook, etc.)
        public abstract PieceType Type { get; }

        // Abstract property to get the color of the piece (e.g., White, Black)
        public abstract Player Color { get; }

        // Property to check if the piece has been moved (default is false)
        public bool HasMoved { get; set; } = false;

        // Abstract method to create a copy of the piece
        public abstract Piece Copy();

        // Abstract method to get possible moves for the piece from a given position
        public abstract IEnumerable<Move> GetMoves(Position from, board board);

        // Helper method to get positions in a given direction, stopping at the edge or when blocked
        protected IEnumerable<Position> MovePositionsInDir(Position from, board board, Direction dir)
        {
            // Start moving in the given direction
            for (Position pos = from + dir; board.IsInside(pos); pos += dir)
            {
                // If the position is empty, it's a valid move
                if (board.IsEmpty(pos))
                {
                    yield return pos;
                    continue;
                }

                // If the position is occupied by an enemy piece, it's a valid move
                Piece piece = board[pos];
                if (piece.Color != Color)
                {
                    yield return pos;
                }

                // Stop further movement if the piece cannot move past another piece
                yield break;
            }
        }

        // Helper method to get positions in multiple directions
        protected IEnumerable<Position> MovePositionsDirs(Position from, board board, Direction[] dirs)
        {
            // Combine the results of moving in all specified directions
            return dirs.SelectMany(dir => MovePositionsInDir(from, board, dir));
        }
        public virtual bool CanCaptureOpponentKing(Position from, board board)
        {
            return GetMoves(from, board).Any(move =>
            {
                Piece piece = board[move.ToPos];
                return piece != null && piece.Type == PieceType.King;
            });

        }
    }
}

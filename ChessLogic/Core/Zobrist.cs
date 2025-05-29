using System;
using ChessLogic; // Assuming Player, PieceType, board, Position are here

namespace ChessLogic.Core.Ai // Assuming this is the correct namespace
{
    public static class ZobristHasher
    {
        private static readonly ulong[,,] pieceHashes = new ulong[2, 6, 64]; // [color, pieceType, square]
        private static readonly ulong whiteToMoveHash;
        private static readonly ulong[] castleRightsHash = new ulong[4]; // [WhiteKS, WhiteQS, BlackKS, BlackQS]
        private static readonly ulong[] enPassantFileHash = new ulong[8]; // Hash for en passant file (if any)
                                                                          // One for each file, could also do per square if preferred

        private static readonly Random rng = new Random(1337); // fixed seed for consistent hashes

        static ZobristHasher()
        {
            for (int color = 0; color < 2; color++)
            {
                for (int type = 0; type < 6; type++)
                {
                    for (int sq = 0; sq < 64; sq++)
                    {
                        pieceHashes[color, type, sq] = RandomUlong();
                    }
                }
            }

            whiteToMoveHash = RandomUlong();
            for (int i = 0; i < 4; i++)
            {
                castleRightsHash[i] = RandomUlong();
            }
            for (int i = 0; i < 8; i++)
            {
                enPassantFileHash[i] = RandomUlong();
            }
        }

        private static ulong RandomUlong()
        {
            byte[] buffer = new byte[8];
            rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong ComputeHash(board board, Player currentPlayer)
        {
            ulong hash = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board[row, col];
                    if (piece == null) continue;

                    int colorIdx = piece.Color == Player.White ? 0 : 1;
                    int typeIdx = piece.Type switch
                    {
                        PieceType.Pawn => 0,
                        PieceType.Knight => 1,
                        PieceType.Bishop => 2,
                        PieceType.Rook => 3,
                        PieceType.Queen => 4,
                        PieceType.King => 5,
                        _ => throw new ArgumentOutOfRangeException(nameof(piece.Type), "Invalid piece type for hashing") // Should not happen
                    };
                    int squareIdx = row * 8 + col;
                    hash ^= pieceHashes[colorIdx, typeIdx, squareIdx];
                }
            }

            if (currentPlayer == Player.White)
            {
                hash ^= whiteToMoveHash;
            }

            // Castling Rights
            if (board.CastleRightKS(Player.White)) hash ^= castleRightsHash[0];
            if (board.CastleRightQS(Player.White)) hash ^= castleRightsHash[1];
            if (board.CastleRightKS(Player.Black)) hash ^= castleRightsHash[2];
            if (board.CastleRightQS(Player.Black)) hash ^= castleRightsHash[3];

            // En Passant Target Square
            // The player whose turn it *was* (opponent of currentPlayer) is the one who might have just made a pawn skip.
            Player opponent = currentPlayer.Opponent();
            Position enPassantTargetSquare = board.GetPawnSkipPosition(opponent); // Get EP square vulnerable to currentPlayer
            if (enPassantTargetSquare != null)
            {
                // We only care about the file of the pawn that *could* be captured.
                // The actual en passant square (where the capturing pawn moves) is what matters.
                // board.GetPawnSkipPosition(player) returns the square *behind* the pawn that moved two steps.
                // This is the square the capturing pawn moves TO.
                hash ^= enPassantFileHash[enPassantTargetSquare.Column];
            }

            return hash;
        }
    }
}
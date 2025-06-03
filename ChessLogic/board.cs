using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLogic
{
    public class board
    {
        private readonly Piece[,] pieces = new Piece[8, 8];

        private readonly Dictionary<Player, Position> pawnSkipPositions = new Dictionary<Player, Position>
        {
            { Player.White, null },
            { Player.Black, null }
        };

        public Piece this[int row, int col]
        {
            get
            {
                if (row < 0 || row >= 8 || col < 0 || col >= 8)
                    return null;
                return pieces[row, col];
            }
            set
            {
                if (row >= 0 && row < 8 && col >= 0 && col < 8)
                    pieces[row, col] = value;
            }
        }

        public Piece this[Position pos]
        {
            get
            {
                if (!IsInside(0, pos))
                    throw new Exception($"Invalid board access: {pos.Row}, {pos.Column}");
                return pieces[pos.Row, pos.Column];
            }
            set
            {
                if (!IsInside(0, pos))
                    throw new Exception($"Invalid board access: {pos.Row}, {pos.Column}");
                pieces[pos.Row, pos.Column] = value;
            }
        }


        public Position GetPawnSkipPosition(Player player)
        {
            return pawnSkipPositions[player];
        }

        public void SetPawnSkipPosition(Player player, Position pos)
        {
            pawnSkipPositions[player] = pos;
        }

        public static board Initial()
        {
            board board = new board();
            board.AddStartPieces();
            return board;
        }

        private void AddStartPieces()
        {
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2] = new Bishop(Player.Black);
            this[0, 3] = new Queen(Player.Black);
            this[0, 4] = new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6] = new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            for (int c = 0; c < 8; c++)
            {
                this[1, c] = new Pawn(Player.Black);
                this[6, c] = new Pawn(Player.White);
            }
        }

        public static bool IsInside(int frontRow, Position pos)
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }

        public bool IsEmpty(Position pos)
        {
            return this[pos] == null;
        }

        public IEnumerable<Position> PiecePositions()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);
                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        public IEnumerable<Position> PiecePositionsFor(Player player)
        {
            return PiecePositions().Where(pos => this[pos].Color == player);
        }

        public bool IsInCheck(Player player)
        {
            return PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Piece piece = this[pos];
                return piece != null && piece.CanCaptureOpponentKing(pos, this);
            });
        }

        public board Copy()
        {
            board copy = new board();
            foreach (Position pos in PiecePositions())
            {
                copy[pos] = this[pos].Copy();
            }
            return copy;
        }

        public Counting CountPieces()
        {
            Counting counting = new Counting();
            foreach (Position pos in PiecePositions())
            {
                Piece piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }
            return counting;
        }

        public bool InsufficientMaterial()
        {
            Counting counting = CountPieces();
            return IsKingVKing(counting) ||
                   IsKingBishopVKing(counting) ||
                   IsKingKnightVKing(counting) ||
                   IsKingBishopVKingBishop(counting);
        }

        private static bool IsKingVKing(Counting counting)
        {
            return counting.TotalCount == 2;
        }

        private static bool IsKingBishopVKing(Counting counting)
        {
            return counting.TotalCount == 3 &&
                   (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);
        }

        private static bool IsKingKnightVKing(Counting counting)
        {
            return counting.TotalCount == 3 &&
                   (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
        }

        private bool IsKingBishopVKingBishop(Counting counting)
        {
            if (counting.TotalCount != 4) return false;
            if (counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1) return false;

            Position wBishopPos = FindPiece(Player.White, PieceType.Bishop);
            Position bBishopPos = FindPiece(Player.Black, PieceType.Bishop);

            return wBishopPos.SquareColor() == bBishopPos.SquareColor();
        }

        private Position FindPiece(Player color, PieceType type)
        {
            return PiecePositionsFor(color).First(pos => this[pos].Type == type);
        }

        private bool IsUnmovedKingAndRook(Position KingPos, Position rookPos)
        {
            if (IsEmpty(KingPos) || IsEmpty(rookPos)) return false;

            Piece king = this[KingPos];
            Piece rook = this[rookPos];

            return king.Type == PieceType.King && rook.Type == PieceType.Rook && !king.HasMoved && !rook.HasMoved;
        }

        public bool CastleRightKS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 7)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
                _ => false
            };
        }

        public bool CastleRightQS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 0)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 0)),
                _ => false
            };
        }

        private bool HasPawnInPosition(Player player, Position[] pawnPositions, Position skipPos)
        {
            foreach (Position pos in pawnPositions.Where(pos => IsInside(0, pos)))
            {
                Piece piece = this[pos];
                if (piece == null || piece.Color != player || piece.Type != PieceType.Pawn) continue;

                EnPassant move = new EnPassant(pos, skipPos);
                if (move.IsLegal(this)) return true;
            }
            return false;
        }

        public bool CanCaptureEnPassant(Player player)
        {
            Position skipPos = GetPawnSkipPosition(player.Opponent());
            if (skipPos == null) return false;

            Position[] pawnPositions = player switch
            {
                Player.White => new Position[] { skipPos + Direction.SouthWest, skipPos + Direction.SouthEast },
                Player.Black => new Position[] { skipPos + Direction.NorthWest, skipPos + Direction.NorthEast },
                _ => Array.Empty<Position>()
            };

            return HasPawnInPosition(player, pawnPositions, skipPos);
        }

        public bool IsSquareAttacked(Position square, Player attacker)
        {
            // Check pawn attacks
            int pawnDir = attacker == Player.White ? -1 : 1;
            Position[] pawnAttacks =
            {
        new Position(square.Row + pawnDir, square.Column - 1),
        new Position(square.Row + pawnDir, square.Column + 1)
    };

            foreach (Position pos in pawnAttacks)
            {
                if (IsInside(0, pos) &&
                    this[pos] is Pawn pawn &&
                    pawn.Color == attacker)
                {
                    return true;
                }
            }

            // Check bishop/queen diagonals
            Direction[] bishopDirs =
            {
        Direction.NorthWest, Direction.NorthEast,
        Direction.SouthWest, Direction.SouthEast
    };

            foreach (Direction dir in bishopDirs)
            {
                for (int i = 1; i < 8; i++)
                {
                    Position pos = square + i * dir;
                    if (!IsInside(0, pos)) break;

                    Piece piece = this[pos];
                    if (piece == null) continue;

                    if (piece.Color == attacker &&
                        (piece.Type == PieceType.Bishop ||
                         piece.Type == PieceType.Queen))
                    {
                        return true;
                    }
                    break; // Blocked path
                }
            }

            // ... add other piece checks ...
            return false;
        }
        public int CountAttackers(Position square, Player attackerColor)
        {
            int count = 0;
            foreach (Position pos in PiecePositionsFor(attackerColor))
            {
                Piece piece = this[pos];
                if (piece == null) continue;

                foreach (Move move in piece.GetMoves(pos, this, true))
                {
                    if (move.ToPos.Equals(square))
                    {
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

    }
}

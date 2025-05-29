using System;

namespace ChessLogic.Moves
{
    public class Castle : Move
    {
        public override MoveType Type { get; }
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        private readonly Direction kingMoveDir;
        private readonly Position rookFromPos;
        private readonly Position rookToPos;

        public Castle(MoveType type, Position kingpos)
        {
            Type = type;
            FromPos = kingpos;

            if (type == MoveType.CastleKS)
            {
                kingMoveDir = Direction.East;
                ToPos = new Position(kingpos.Row, 6);
                rookFromPos = new Position(kingpos.Row, 7);
                rookToPos = new Position(kingpos.Row, 5);
            }
            else if (type == MoveType.CastleQS)
            {
                kingMoveDir = Direction.West;
                ToPos = new Position(kingpos.Row, 2);
                rookFromPos = new Position(kingpos.Row, 0);
                rookToPos = new Position(kingpos.Row, 3);
            }
        }

        public override bool Execute(board board)
        {
            new NormalMove(FromPos, ToPos).Execute(board);
            new NormalMove(rookFromPos, rookToPos).Execute(board);
            return false;
        }

        public override bool IsLegal(board board)
        {
            Player player = board[FromPos].Color;

            // Must have castling rights
            if ((Type == MoveType.CastleKS && !board.CastleRightKS(player)) ||
                (Type == MoveType.CastleQS && !board.CastleRightQS(player)))
                return false;

            // Squares between king and rook must be empty
            int startCol = Math.Min(FromPos.Column, rookFromPos.Column) + 1;
            int endCol = Math.Max(FromPos.Column, rookFromPos.Column) - 1;

            for (int col = startCol; col <= endCol; col++)
            {
                if (!board.IsEmpty(new Position(FromPos.Row, col)))
                    return false;
            }

            // King must not be in check
            if (board.IsInCheck(player))
                return false;

            // Squares king passes through must not be attacked
            Position[] kingPath = new Position[]
            {
                FromPos,
                FromPos + kingMoveDir,
                ToPos
            };

            foreach (var square in kingPath)
            {
                if (board.IsSquareAttacked(square, player.Opponent()))
                    return false;
            }

            return true;
        }

    }
}

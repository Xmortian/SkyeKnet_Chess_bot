namespace ChessLogic
{
    public class NormalMove : Move
    {
        public override MoveType Type => MoveType.Normal;
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        public NormalMove(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
        }

        public override bool Execute(board board)
        {
            Piece piece = board[FromPos];
            bool capture = !board.IsEmpty(ToPos);
            board[ToPos] = piece;
            board[FromPos] = null;
            piece.HasMoved = true;

            return capture || piece.Type == PieceType.Pawn;
        }
        public override bool IsLegal(board board)
        {
            Piece movingPiece = board[FromPos];
            Piece targetPiece = board[ToPos];

            // Can't move if own piece is at the destination
            if (targetPiece != null && targetPiece.Color == movingPiece.Color)
                return false;

            return true;
        }

    }
}
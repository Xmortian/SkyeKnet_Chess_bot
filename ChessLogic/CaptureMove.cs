namespace ChessLogic
{
    public class CaptureMove : Move
    {
        public override MoveType Type => MoveType.Normal;
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        public CaptureMove(Position from, Position to)
        {
            FromPos = from;
            ToPos = to;
        }

        public override bool Execute(board board)
        {
            board[ToPos] = board[FromPos];
            board[FromPos] = null;

            return true;
        }


        public override bool IsLegal(board board)
        {
            return !board.IsEmpty(ToPos) && board[ToPos].Color != board[FromPos].Color;
        }
    }
}
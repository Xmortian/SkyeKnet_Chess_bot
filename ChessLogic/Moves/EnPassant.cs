namespace ChessLogic
{
    public class EnPassant : Move
    {
        public override MoveType Type => MoveType.EnPassant;

        public override Position FromPos { get; }

        public override Position ToPos { get; }

        public readonly Position capturePos;

        public EnPassant(Position from,  Position to)
        {
            FromPos = from;
            ToPos = to;
            capturePos = new Position(from.Row, to.Column);

        }

        public override bool Execute(board board)
        {
            new NormalMove(FromPos, ToPos).Execute(board);
            board[capturePos]= null;
            return true;
        }
    }
}

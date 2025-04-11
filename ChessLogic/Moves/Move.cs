namespace ChessLogic
{
    public abstract class Move
    {
        public abstract MoveType Type { get; }  // Defines the type of move (e.g., normal, castling, etc.)
        public abstract Position FromPos { get; } // Stores the starting position of the move
        public abstract Position ToPos { get; } // Stores the ending position of the move

        public abstract bool Execute(board board); // Executes the move on the given board
        public virtual bool IsLegal(board board)
        {
            Player player = board[FromPos].Color;
            board boardCopy = board.Copy();                 // NEED TO CHANGE THIS WHEN IMPLEMENTING CHESS ENGINE
            Execute(boardCopy);
            return !boardCopy.IsInCheck(player);
        }



    }
}

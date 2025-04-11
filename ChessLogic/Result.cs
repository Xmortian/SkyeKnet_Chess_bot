namespace ChessLogic
{
    public class Result
    {
        public Player Winner { get;  }

        public EndReasonn Reasonn { get; }

        public Result(Player winner, EndReasonn reasonn)

        {
            Winner = winner;
            Reasonn = reasonn;
        }
        public static Result Win(Player winner)
        {
            return new Result(winner,EndReasonn.Checkmate);

        }
        public static Result Draw(EndReasonn reasonn)
        {
            return new Result(Player.None, reasonn);
        }
    }
}

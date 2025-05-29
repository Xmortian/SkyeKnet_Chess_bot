using ChessLogic;

namespace Chess.AI
{
    public static class BoardExtensions
    {
=        public static Player CurrentPlayer(this StateOfGame state)
        {
            return state.CurrentPlayer;
        }
        public static Player Opponent(this StateOfGame state)
        {
            return state.CurrentPlayer.Opponent();
        }
    }
}
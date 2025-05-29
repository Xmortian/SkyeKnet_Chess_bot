namespace ChessLogic
{
    // Enum representing the possible players in the game: None, White, or Black
    public enum Player
    {
        None,   // No player (empty space)
        White,  
        Black   
    }

    public static class PlayerExtensions
    {
        public static Player Opponent(this Player player)
        {
            
            return player switch
            {
                Player.White => Player.Black, 
                Player.Black => Player.White, 
                _ => Player.None,             // If the player is None, return None (no opponent)
            };
        }
    }
}

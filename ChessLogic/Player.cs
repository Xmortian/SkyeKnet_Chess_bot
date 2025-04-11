namespace ChessLogic
{
    // Enum representing the possible players in the game: None, White, or Black
    public enum Player
    {
        None,   // No player (empty space)
        White,  // White player
        Black   // Black player
    }

    // Static class to extend the Player enum with additional functionality
    public static class PlayerExtensions
    {
        // Method to get the opponent of the current player
        public static Player Opponent(this Player player)
        {
            // Return the opponent based on the current player
            return player switch
            {
                Player.White => Player.Black, // White's opponent is Black
                Player.Black => Player.White, // Black's opponent is White
                _ => Player.None,             // If the player is None, return None (no opponent)
            };
        }
    }
}

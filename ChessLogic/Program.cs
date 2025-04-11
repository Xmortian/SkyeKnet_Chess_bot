using System;
using ChessLogic;

class Program
{
    static void Main()
    {
        try
        {
            // Initialize the board with starting position
            board chessBoard = board.Initial();

            // Create game state
            StateOfGame gameState = new StateOfGame(Player.White, chessBoard);

            // Basic console output
            Console.WriteLine("Chess Game Initialized Successfully");
            Console.WriteLine($"Current Player: {gameState.CurrentPlayer}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.ReadKey();
        }
    }
}
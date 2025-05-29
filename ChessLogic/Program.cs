using System;
using ChessLogic;

class Program
{
    static void Main()
    {
        try
        {
            
            board chessBoard = board.Initial();

            
            StateOfGame gameState = new StateOfGame(Player.White, chessBoard);

            
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
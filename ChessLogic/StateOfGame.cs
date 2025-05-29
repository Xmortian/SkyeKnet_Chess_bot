using System;
using System.Collections.Generic;
using System.Linq;
using ChessLogic.Core;

namespace ChessLogic
{
    public class StateOfGame
    {
        public board Board { get; }
        public Player CurrentPlayer { get; private set; }

        public Result result { get; private set; } = null;
        private int noCaptureOrPawnMoves = 0;
        public string stateString;

        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();

        public StateOfGame(Player player, board board)
        {
            CurrentPlayer = player;
            Board = board;

            stateString = new StateString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            if (!board.IsInside(0, pos) || Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
                return Enumerable.Empty<Move>();

            Piece piece = Board[pos];
            IEnumerable<Move> moveCandidates = piece.GetMoves(pos, Board);
            return moveCandidates.Where(move => IsMoveSafe(move));
        }

        private bool IsMoveSafe(Move move)
        {
            board boardCopy = Board.Copy();
            move.Execute(boardCopy); 
            return !boardCopy.IsInCheck(CurrentPlayer);
        }

        public void MakeMove(Move move, ChessClock clock = null) 
        {
            if (IsGameOver())
            {
                Console.WriteLine("Move ignored: Game Over!");
                return;
            }

            Board.SetPawnSkipPosition(CurrentPlayer, null);
            bool captureOrPawn = move.Execute(Board);

            if (captureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
                stateHistory.Clear();
            }
            else
            {
                noCaptureOrPawnMoves++;
            }

            clock?.SwitchTurn(CurrentPlayer.Opponent()); 

            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();
            CheckForGameOver(clock); 
        }

        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            return Board.PiecePositionsFor(player)
                .SelectMany(pos =>
                {
                    Piece piece = Board[pos];
                    return piece.GetMoves(pos, Board);
                })
                .Where(move => IsMoveSafe(move));
        }

        private void CheckForGameOver(ChessClock clock = null) 
        {
            Console.WriteLine("Checking for game over...");

            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Console.WriteLine("Checkmate! Winner: " + CurrentPlayer.Opponent());
                    result = Result.Win(CurrentPlayer.Opponent());
                }
                else
                {
                    Console.WriteLine("Game Drawn: Stalemate");
                    result = Result.Draw(EndReasonn.Stalemate);
                }

                EndGame();
                clock?.StopClock(); 
                return;
            }

            if (Board.InsufficientMaterial())
            {
                Console.WriteLine("Game Drawn: Insufficient Material!");
                result = Result.Draw(EndReasonn.InsufficientMaterial);
                EndGame();
                clock?.StopClock();
            }
            else if (FiftyMoveRule())
            {
                result = Result.Draw(EndReasonn.FiftyMoveRule);
                EndGame();
                clock?.StopClock();
            }
            else if (ThreefoldRepetition())
            {
                result = Result.Draw(EndReasonn.ThreeFoldRepetition);
                EndGame();
                clock?.StopClock();
            }
        }

        public bool IsGameOver()
        {
            return result != null;
        }

        private void EndGame()
        {
            //Console.WriteLine("Game Over: " + result.EndReasonn);
        }

        private bool FiftyMoveRule()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves == 50;
        }

        private void UpdateStateString()
        {
            stateString = new StateString(CurrentPlayer, Board).ToString();
            if (!stateHistory.ContainsKey(stateString))
                stateHistory[stateString] = 1;
            else
                stateHistory[stateString]++;
        }

        private bool ThreefoldRepetition()
        {
            return stateHistory[stateString] == 3;
        }

        internal IEnumerable<object> GenerateLegalMoves()
        {
            throw new NotImplementedException();
        }
    }
}

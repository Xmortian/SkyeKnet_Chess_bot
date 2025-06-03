using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ChessLogic;
using ChessLogic.Core.Ai; 


namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private readonly Random rng = new Random();
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();
        private StateOfGame gameState;
        private Position selectedPos = null;
        private readonly ChessAI ai = new ChessAI(Player.Black, 15); 
        private ChessClock clock;
        private DispatcherTimer uiTimer;
        private Move lastMovePlayed;
        private int currentGameInstanceId = 0; // <<< ADD THIS LINE

        private bool isBotSpeaking = false;
        private Queue<string> botDialogueQueue = new Queue<string>();


        private DispatcherTimer typingTimer;
        private string fullDialogueText = "";
        private int typingIndex = 0;
        private DispatcherTimer dialogueDisplayTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            gameState = new StateOfGame(Player.White, board.Initial());
            clock = new ChessClock();
            StartClockDisplay();

            BotDialogue.ResetForNewGame();

            DrawBoard(gameState.Board);
            ShowBotDialogue(null, Player.Black); // Ensure greeting shown at game start


        }

        private void StartClockDisplay()
        {
            uiTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            uiTimer.Tick += (s, e) =>
            {
                WhiteClockText.Text = clock.GetTimeString(Player.White);
                BlackClockText.Text = clock.GetTimeString(Player.Black);

                TimeSpan wt = clock.WhiteTime;
                TimeSpan bt = clock.BlackTime;

                WhiteClockText.Foreground = wt.TotalSeconds <= 10 ? Brushes.Red : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5B358"));
                BlackClockText.Foreground = bt.TotalSeconds <= 10 ? Brushes.Red : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5B358"));

                if (clock.IsOutOfTime(Player.White))
                {
                    uiTimer.Stop();
                    MessageBox.Show("White loses on time!");
                    
                    ShowGameOverMessage(); 
                }
                else if (clock.IsOutOfTime(Player.Black))
                {
                    uiTimer.Stop();
                    MessageBox.Show("Black loses on time!");
                    
                    ShowGameOverMessage(); 
                }
            };

            uiTimer.Start();
        }

        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle
                    {
                        Fill = Brushes.Transparent,
                        Stroke = Brushes.Cyan,
                        StrokeThickness = 0.5
                    };
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    
                    pieceImages[r, c].Source = Images.GetImage(piece); 
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gameState.IsGameOver() || gameState.CurrentPlayer != Player.White)
                return;

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
                OnFromPositionSelected(pos);
            else
                OnToPositionSelected(pos);
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            if (gameState.IsGameOver() || gameState.CurrentPlayer != Player.White)
                return;

            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            if (gameState.IsGameOver() || gameState.CurrentPlayer != Player.White)
                return;

            selectedPos = null;
            HideHighlights();

            if (moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                    HandlePromotion(move.FromPos, move.ToPos);
                else
                    HandleMove(move);
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            
            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;
            MenuContainer.Visibility = Visibility.Visible;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Visibility = Visibility.Collapsed;
                MenuContainer.Content = null;
                
                Move promMove = new PawnPromotion(from, to, type); 
                HandleMove(promMove);
            };
        }

        private void HandleMove(Move move)
        {
            if (clock.IsOutOfTime(gameState.CurrentPlayer))
            {
                lastMovePlayed = move;

                MessageBox.Show($"{gameState.CurrentPlayer} loses on time!");
                uiTimer.Stop();
                ShowGameOverMessage();

                return;
            }

            if (!clock.IsRunning && gameState.CurrentPlayer == Player.White) 
            {
                 clock.StartTurn(Player.White);
            }
            else if (!clock.IsRunning && gameState.CurrentPlayer == Player.Black) 
            {
                clock.StartTurn(Player.Black);
            }


            gameState.MakeMove(move);
            moveCache.Clear();
            DrawBoard(gameState.Board);
            HideHighlights();
            
            
            
            ShowBotDialogue(move, Player.Black);
            lastMovePlayed = move;

            if (!gameState.IsGameOver())
            {
                clock.SwitchTurn(gameState.CurrentPlayer); 
                TryBotMove();
            }
            else
            {
                ai.NotifyGameEnded(gameState.result.Winner == Player.Black);
                ShowGameOverMessage();
                return;
            }

        }

        private async void TryBotMove()
        {
            if (gameState.IsGameOver()) 
            {
                ShowGameOverMessage();
                return;
            }
            if (clock.IsOutOfTime(gameState.CurrentPlayer)) 
            {
                MessageBox.Show($"{gameState.CurrentPlayer} loses on time!");
                uiTimer.Stop();
                ShowGameOverMessage();
                return;
            }

            if (gameState.CurrentPlayer == Player.Black) 
            {
                if (!clock.IsRunning) clock.StartTurn(Player.Black); 

                ai.NotifyOpponentMoved(); 
                Move aiMove = await Task.Run(() => ai.GetBestMove(gameState)); 

                
                if (clock.IsOutOfTime(Player.Black)) 
                {
                    MessageBox.Show($"{Player.Black} loses on time!");
                    uiTimer.Stop();
                    ShowGameOverMessage();
                    return;
                }

                if (aiMove != null)
                {
                    gameState.MakeMove(aiMove);
                    lastMovePlayed = aiMove; 

                    DrawBoard(gameState.Board);
                    HideHighlights();

                    ShowHighlights(); 

                    ShowBotDialogue(aiMove, Player.Black); 
                }
                
                if (!gameState.IsGameOver())
                {
                    clock.SwitchTurn(gameState.CurrentPlayer); 
                }
                else
                {
                     ShowGameOverMessage();
                }
            }
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
                moveCache[move.ToPos] = move;
        }

        private void ShowHighlights()
        {
            if (gameState.IsGameOver()) return;

            // Highlight last move if available
            if (lastMovePlayed != null)
            {
                Color lastMoveColor = Color.FromArgb(70, 245, 225, 100); // golden soft tone
                highlights[lastMovePlayed.FromPos.Row, lastMovePlayed.FromPos.Column].Fill = new SolidColorBrush(lastMoveColor);
                highlights[lastMovePlayed.ToPos.Row, lastMovePlayed.ToPos.Column].Fill = new SolidColorBrush(lastMoveColor);
            }

            Color moveColor = Color.FromArgb(120, 30, 60, 200);
            foreach (Position to in moveCache.Keys)
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(moveColor);


        }


        private void HideHighlights()
        {
            List<Position> keysToClear = new List<Position>(moveCache.Keys); 
            foreach (Position to in highlights.OfType<Rectangle>().Select((r, i) => new { Row = i / 8, Col = i % 8 })
                                        .Select(coords => new Position(coords.Row, coords.Col)))
            {
                 highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
            foreach (Position to in keysToClear)
            {
                if (to != null && highlights[to.Row, to.Column] != null) 
                   highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void ShowGameOverMessage()
        {
            if (!gameState.IsGameOver()) return;

            StopAllTimers();

            Result result = gameState.result;
            string message = result.Reasonn switch
            {
                EndReasonn.Checkmate => $"{(result.Winner == Player.White ? "White" : "Black")} wins by checkmate!",
                EndReasonn.Stalemate => "Draw by stalemate.",
                EndReasonn.FiftyMoveRule => "Draw by fifty-move rule.",
                EndReasonn.InsufficientMaterial => "Draw due to insufficient material.",
                EndReasonn.ThreeFoldRepetition => "Draw by threefold repetition.",
                _ => "Game Over"
            };

            // Calculate bot perspective win/loss
            Player botPerspective = Player.Black;
            bool isBotWin = result.Winner == botPerspective;
            bool isBotLoss = result.Winner == botPerspective.Opponent();

            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

            // Directly trigger bot ending dialogue
            TriggerBotEndDialogue(isBotWin, isBotLoss);
        }

        private void TriggerBotEndDialogue(bool isBotWin, bool isBotLoss)
        {
            if (!isBotWin && !isBotLoss) return;  // Draw situations

            GamePhase phase = EvaluatePhase(gameState);
            int eval = Evaluator.Evaluate(gameState.Board, Player.Black);

            string line = BotDialogue.GetBotLine(
                phase,
                eval,
                false, // opponentTookLong
                lastMovePlayed,
                gameState.Board,
                Player.Black, // aiPlayer
                isBotWin,
                isBotLoss
            );

            if (!string.IsNullOrWhiteSpace(line))
            {
                // Clear any existing dialogue
                botDialogueQueue.Clear();
                isBotSpeaking = false;

                // Show immediately
                StartTypingAnimation(line);
            }
        }

        private void StopAllTimers()
        {
            uiTimer?.Stop();
            typingTimer?.Stop();
            dialogueDisplayTimer?.Stop();
            clock.StopClock();
        }

        private void ShowBotDialogue(Move lastMove, Player perspectivePlayer)
        {


            if (isBotSpeaking)
                return; // Don’t interrupt existing dialogue

            GamePhase phase = EvaluatePhase(gameState);
            int eval = Evaluator.Evaluate(gameState.Board, perspectivePlayer);

            bool isBotWin = gameState.IsGameOver() && gameState.result?.Winner == perspectivePlayer;
            bool isBotLoss = gameState.IsGameOver() && gameState.result?.Winner == perspectivePlayer.Opponent();
            bool opponentTookLong = false;

            string line = BotDialogue.GetBotLine(
                phase,
                eval,
                opponentTookLong,
                lastMove,
                gameState.Board,
                perspectivePlayer,
                isBotWin,
                isBotLoss
            );

            if (!string.IsNullOrWhiteSpace(line))
            {
                StartTypingAnimation(line);
            }


        }
        private void QueueBotEndingDialogue(Player botPerspective)
        {
            GamePhase phase = EvaluatePhase(gameState);
            int eval = Evaluator.Evaluate(gameState.Board, botPerspective);
            bool isBotWin = gameState.result?.Winner == botPerspective;
            bool isBotLoss = gameState.result?.Winner == botPerspective.Opponent();

            string line = BotDialogue.GetBotLine(
                phase,
                eval,
                false,
                lastMovePlayed,
                gameState.Board,
                botPerspective,
                isBotWin,
                isBotLoss
            );

            if (!string.IsNullOrWhiteSpace(line))
            {
                botDialogueQueue.Enqueue(line);

                if (!isBotSpeaking)
                {
                    // Kick off the queue manually if nothing is talking
                    string nextLine = botDialogueQueue.Dequeue();
                    StartTypingAnimation(nextLine);
                }
            }
        }






        private GamePhase EvaluatePhase(StateOfGame state)
        {
            
            int pieceCount = 0;

            
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    
                    if (state.Board[r, c] != null)
                    {
                        pieceCount++;
                    }
                }
            }

            
            
            if (pieceCount > 26)
            {
                return GamePhase.Opening;
            }
            
            else if (pieceCount <= 12)
            {
                return GamePhase.Endgame;
            }
            
            else
            {
                return GamePhase.Middlegame;
            }
        }
        private void StartTypingAnimation(string line)
        {
            isBotSpeaking = true;
            fullDialogueText = line;
            typingIndex = 0;
            BotDialogueText.Text = "";
            BotDialogueText.Visibility = Visibility.Visible;

            typingTimer?.Stop();
            typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(35)
            };

            typingTimer.Tick += (s, e) =>
            {
                if (typingIndex < fullDialogueText.Length)
                {
                    BotDialogueText.Text += fullDialogueText[typingIndex];
                    typingIndex++;
                }
                else
                {
                    typingTimer.Stop();
                    StartDialogueClearTimer();
                }
            };

            typingTimer.Start();
        }


        private void StartDialogueClearTimer()
        {
            dialogueDisplayTimer?.Stop();
            dialogueDisplayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4.5)
            };

            dialogueDisplayTimer.Tick += (s, e) =>
            {
                dialogueDisplayTimer.Stop();
                BotDialogueText.Text = "";
                BotDialogueText.Visibility = Visibility.Collapsed;
                isBotSpeaking = false;

                if (botDialogueQueue.Count > 0)
                {
                    string nextLine = botDialogueQueue.Dequeue();
                    StartTypingAnimation(nextLine);
                }
            };

            dialogueDisplayTimer.Start();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void StartOver_Click(object sender, RoutedEventArgs e)
        {
            //StopAllTimers();                         // Stop UI + Clock timers
            //currentGameInstanceId++; // <<< ADD THIS LINE (increment for new game)

            //// Reset Core Game Logic States
            //BotDialogue.ResetForNewGame();           // Reset bot dialogue intro flags [from BotDialogue.cs, e.g., source 52]
            //TranspositionTable.Clear();              // Clear the transposition table
            //MoveOrdering.ClearHistoryAndKillers();   // Clear killer moves and history heuristic

            //gameState = new StateOfGame(Player.White, board.Initial()); // Fresh board & turn
            //clock = new ChessClock();                // New clock instance

            //// Reset AI specific state
            //ai.NotifyOpponentMoved();                // Reset AI thinking time
            //ai.SetLastOpponentMove(null);            // Clear old move history if any for AI
            //ai.ResetForNewGame(); // Add this line

            //// Reset MainWindow specific game state
            //lastMovePlayed = null;                   // Clear the last move played in the UI
            //selectedPos = null;                    // Clear any piece selection
            //moveCache.Clear();                     // Clear cached moves for any selected piece
            //isBotSpeaking = false;                 // Reset bot speaking flag
            //// if (botDialogueQueue != null) botDialogueQueue.Clear(); // If you use a queue

            //// Update UI
            //DrawBoard(gameState.Board);              // Re-draw fresh board
            //HideHighlights();                        // Ensure all highlights are cleared
            //StartClockDisplay();                     // Restart clock visuals

            BotDialogueText.Text = "";                 // Clear any lingering bot message
            BotDialogueText.Visibility = Visibility.Collapsed; // Hide the dialogue text area

            // Trigger initial bot dialogue for the new game
            ShowBotDialogue(null, Player.Black);     // Show initial greeting
        }
        private void QueueBotDialogue(Player perspective)
        {
            GamePhase phase = EvaluatePhase(gameState);
            int eval = Evaluator.Evaluate(gameState.Board, perspective);
            string line = BotDialogue.GetBotLine(
                phase,
                eval,
                false,
                lastMovePlayed,
                gameState.Board,
                perspective,
                gameState.result?.Winner == perspective,
                gameState.result?.Winner == perspective.Opponent()
            );

            if (!string.IsNullOrWhiteSpace(line))
            {
                if (!isBotSpeaking)
                    StartTypingAnimation(line);
                else
                    botDialogueQueue.Enqueue(line);
            }
        }


    }
}

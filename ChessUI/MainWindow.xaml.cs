using System;
using System.Collections.Generic;
using System.Linq;
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
            DrawBoard(gameState.Board);
            
            BotDialogue.ResetForNewGame(); 
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

            if (!gameState.IsGameOver())
            {
                clock.SwitchTurn(gameState.CurrentPlayer); 
                TryBotMove();
            }
            else
            {
                ShowGameOverMessage();
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
                    DrawBoard(gameState.Board);
                    HideHighlights();
                    
                    
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

            Color color = Color.FromArgb(120, 30, 60, 200);
            foreach (Position to in moveCache.Keys)
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
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

            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            
            ShowBotDialogue(null, Player.Black); 
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
            GamePhase phase = EvaluatePhase(gameState);
            int eval = Evaluator.Evaluate(gameState.Board, perspectivePlayer); 
            
            bool isBotWin = gameState.IsGameOver() && gameState.result != null && gameState.result.Winner == perspectivePlayer;
            bool isBotLoss = gameState.IsGameOver() && gameState.result != null && gameState.result.Winner == perspectivePlayer.Opponent();
            
            
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
                StartTypingAnimation(line);
            else
                BotDialogueText.Text = ""; 
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
            fullDialogueText = line;
            typingIndex = 0;
            BotDialogueText.Text = "";
            BotDialogueText.Visibility = Visibility.Visible;

            typingTimer?.Stop();
            typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(40)
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
                Interval = TimeSpan.FromSeconds(5) 
            };

            dialogueDisplayTimer.Tick += (s, e) =>
            {
                dialogueDisplayTimer.Stop();
                BotDialogueText.Text = "";
                BotDialogueText.Visibility = Visibility.Collapsed;
            };

            dialogueDisplayTimer.Start();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();
        private StateOfGame gameState;
        private Position selectedPos = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            gameState = new StateOfGame(Player.White, board.Initial());
            DrawBoard(gameState.Board);
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
                        StrokeThickness = .5
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
                    int uiRow = 7 - r;
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gameState.IsGameOver()) return; // 🔹 Block input if game over

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col); // Direct mapping
        }

        private void OnFromPositionSelected(Position pos)
        {
            if (gameState.IsGameOver()) return; // 🔹 Stop move selection if game over

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
            if (gameState.IsGameOver()) return; // 🔹 Stop move execution if game over

            selectedPos = null;
            HideHighlights();

            if (moveCache.TryGetValue(pos, out Move move))
            {
                Console.WriteLine($"Move type: {move.Type}"); // Debug

                if (move.Type == MoveType.PawnPromotion)
                {
                    Console.WriteLine("Promotion detected!"); // Debug
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
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
            if (gameState.IsGameOver()) return; // 🔹 Prevent moves if game over

            gameState.MakeMove(move);
            moveCache.Clear();
            DrawBoard(gameState.Board); // Force UI update
            HideHighlights(); // 🔹 Remove highlights after move
            Console.WriteLine($"Moved {move.FromPos} to {move.ToPos}"); // Debug
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            if (gameState.IsGameOver()) return; // 🔹 No highlights if game is over

            Color color = Color.FromArgb(120, 30, 60, 200);
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }
    }
}

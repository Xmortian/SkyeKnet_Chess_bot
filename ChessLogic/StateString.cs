using System.Text;

namespace ChessLogic
{
    public  class StateString
    {
        private readonly StringBuilder sb = new StringBuilder();

        public StateString(Player CurrentPlayer, board board)
        {
            //Piece Placement
            AddPiecePlacement(board);
            sb.Append(' ');
            //Current Player
            AddCurrentPlayer(CurrentPlayer);
            sb.Append(' ');
            //Castling Rights
            AddCastlingRights(board);

            sb.Append(' ');
            //EnPassant Data
            AddEnPassant(board, CurrentPlayer);



        }

        public override string ToString()
        {
            return sb.ToString();
        }
        private static char PieceChar(Piece piece)
        {
            char c = piece.Type switch
            {
                PieceType.Pawn => 'p',
                PieceType.Knight => 'n',
                PieceType.Rook => 'r',
                PieceType.Bishop => 'b',
                PieceType.Queen => 'q',
                PieceType.King => 'k',
                _ => ' '
            };

            if (piece.Color == Player.Black)
            {
                return char.ToLower(c);
            }

            return c;
        }
        
        private void AddRowData(board board, int row)
        {
            int empty = 0;
            for (int c = 0; c < 8; c++)
            {
                if (board[row, c] == null)
                {
                    empty++;
                    continue;
                }

                if (empty>0)
                {
                    sb.Append(empty);
                    empty = 0;
                }

                sb.Append(PieceChar(board[row, c]));
            }

            if (empty > 0)
            {
                sb.Append(empty);
            }
        }

        private void AddPiecePlacement(board board)
        {
            for (int r = 0; r < 8; r++)
            {
                if (r!=0)
                {
                    sb.Append('/');
                }
                AddRowData(board, r);
            }
        }

        private void AddCurrentPlayer(Player currentPlayer)
        {
            if (currentPlayer == Player.White)
            {
                sb.Append('w');
            }
            else
            {
                sb.Append('b');
            }
        }

        private void AddCastlingRights(board board)
        {
            bool CastleWKS = board.CastleRightKS(Player.White);
            bool CastleWQS = board.CastleRightQS(Player.White);
            bool CastleBKs = board.CastleRightKS(Player.Black);
            bool CastleBQS = board.CastleRightQS(Player.Black);

            if (!(CastleWKS || CastleWQS || CastleBKs ||  CastleBQS))
            {
                sb.Append('-');
                return;
            }

            if (CastleWKS)
            {
                sb.Append('K');

            }
            if (CastleWQS)
            {
                sb.Append('Q');
            }
            if (CastleBKs)
            {
                sb.Append('k');
            }

            if (CastleBQS)
            {
                sb.Append('q');
            }
        }

        private void AddEnPassant(board board, Player currentPlayer)
        {
            if (!board.CanCaptureEnPassant(currentPlayer))
            { 
                sb.Append('-');
                return;
            }

            Position pos = board.GetPawnSkipPosition(currentPlayer.Opponent());
            char file = (char)('a' + pos.Column);
            int rank = 8 - pos.Column;
            sb.Append(file);
            sb.Append(rank);
        }

    }
}

using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessLogic;


namespace ChessUI
{
    public static class Images
    {
        private static readonly Dictionary<PieceType, ImageSource> whiteSources = new()
        {
            {PieceType.Pawn, LoadImage("Assets/Chess_plt60.png") },
            {PieceType.Bishop, LoadImage("Assets/Chess_blt60.png") },
            {PieceType.King, LoadImage("Assets/Chess_klt60.png") },
            {PieceType.Queen, LoadImage("Assets/Chess_qlt60.png") },
            {PieceType.Rook, LoadImage("Assets/Chess_rlt60.png") },
            {PieceType.Knight, LoadImage("Assets/Chess_nlt60.png") }

        };

        private static readonly Dictionary<PieceType, ImageSource> blackSources = new()
        {
            {PieceType.Pawn, LoadImage("Assets/Chess_pdt60.png") },
            {PieceType.Bishop, LoadImage("Assets/Chess_bdt60.png") },
            {PieceType.King, LoadImage("Assets/Chess_kdt60.png") },
            {PieceType.Queen, LoadImage("Assets/Chess_qdt60.png") },
            {PieceType.Rook, LoadImage("Assets/Chess_rdt60.png") },
            {PieceType.Knight, LoadImage("Assets/Chess_ndt60.png") }
        };

        private static ImageSource LoadImage(string filepath)
        {
            return new BitmapImage(new Uri(filepath, UriKind.Relative));

        }
        public static ImageSource GetImage(Player color,PieceType type)
        {
            return color switch
            {
                Player.White => whiteSources[type],
                Player.Black => blackSources[type],
                _ => null
            };
        }

        public static ImageSource GetImage(Piece piece)
        {
            if (piece == null)
            {
                return null;
            }
            return GetImage(piece.Color, piece.Type);
        }


    }
}

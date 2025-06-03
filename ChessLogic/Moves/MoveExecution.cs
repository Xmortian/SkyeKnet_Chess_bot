using ChessLogic;

namespace Chess.AI
{
    public static class MoveExtensions
    {
        public static bool IsCastle(this Move move)
        {
            return move.Type == MoveType.CastleKS || move.Type == MoveType.CastleQS;
        }

        public static bool IsPromotion(this Move move)
        {
            return move.Type == MoveType.PawnPromotion;
        }

        public static bool IsCapture(this Move move)
        {
            return move.CapturedPiece != PieceType.None; 
        }

        public static int Score(this Move move)
        {
            if (move.IsPromotion())
                return 900;
            if (move.IsCapture())
                return 100;
            return 0;
        }
    }
}

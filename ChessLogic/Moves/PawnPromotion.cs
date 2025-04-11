using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public  class PawnPromotion : Move 
    {
        public override MoveType Type => MoveType.PawnPromotion;

        public override Position FromPos { get; }
        public override Position ToPos { get; }

        public readonly PieceType newType;

        public PawnPromotion(Position fromPos, Position toPos, PieceType newType)
        {
            FromPos = fromPos;
            ToPos = toPos;
            this.newType = newType;
        }

        private Piece CreatePromotionPiece(Player color)
        {
            return newType switch
            {
                PieceType.Knight => new Knight(color),
                PieceType.Rook => new Rook(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Queen => new Queen(color)
            };
        }

        public override bool Execute(board board)
        {
            Piece pawn = board[FromPos];
            board[FromPos] = null;

            Piece promotionPiece = CreatePromotionPiece(pawn.Color);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;

            return true;

        }
    }
}

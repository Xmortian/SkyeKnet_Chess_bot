using System;
using System.Collections.Generic;
using System.Linq; 

namespace ChessLogic
{
    public static class Evaluator
    {
        private const int CheckmateScore = 100000;
        private const int StalemateScore = 0;
        private const int DrawScore = 0;
        private static readonly Dictionary<PieceType, int> PieceValues = new()
        {
            { PieceType.Pawn, 100 },
            { PieceType.Knight, 320 }, 
            { PieceType.Bishop, 335 }, 
            { PieceType.Rook, 515 },
            { PieceType.Queen, 969 },
            { PieceType.King, 19999 } 
        };
        public static int GetPieceValue(PieceType type) 
        {
            return PieceValues.TryGetValue(type, out int value) ? value : 0;
        }
        private static readonly int[,] PawnTable = new int[8, 8]
        { 
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { 70, 70, 70, 70, 70, 70, 70, 70 }, 
            { 20, 20, 30, 40, 40, 30, 20, 20 }, 
            { 10, 10, 20, 35, 35, 20, 10, 10 }, 
            {  5,  5, 10, 30, 30, 10,  5,  5 },
            {  5, -5,-10,  5,  5,-10, -5,  5 }, 
            {  5, 10, 10,-25,-25, 10, 10,  5 }, 
            {  0,  0,  0,  0,  0,  0,  0,  0 }
        };
        private static readonly int[,] KnightTable = new int[8, 8]
        {
            {-50,-40,-30,-30,-30,-30,-40,-50},
            {-40,-20,  0,  5,  5,  0,-20,-40}, 
            {-30,  5, 10, 15, 15, 10,  5,-30},
            {-30,  0, 15, 20, 20, 15,  0,-30}, 
            {-30,  5, 15, 20, 20, 15,  5,-30},
            {-30,  0, 10, 15, 15, 10,  0,-30},
            {-40,-20,  0,  0,  0,  0,-20,-40},
            {-50,-40,-30,-30,-30,-30,-40,-50}
        };
        private static readonly int[,] BishopTable = new int[8, 8]
        {
            {-20,-10,-10,-10,-10,-10,-10,-20},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-10,  5,  5, 10, 10,  5,  5,-10}, 
            {-10,  0, 10, 10, 10, 10,  0,-10},
            {-10, 10, 10, 10, 10, 10, 10,-10}, 
            {-10,  0,  5, 10, 10,  5,  0,-10},
            {-10,  5,  0,  0,  0,  0,  5,-10},
            {-20,-10,-10,-10,-10,-10,-10,-20}
        };
        private static readonly int[,] RookTable = new int[8, 8]
        { 
            {  0,  0,  5, 10, 10,  5,  0,  0}, 
            {  5, 10, 10, 10, 10, 10, 10,  5},
            { -5,  0,  0,  0,  0,  0,  0, -5},
            { -5,  0,  0,  0,  0,  0,  0, -5},
            { -5,  0,  0,  0,  0,  0,  0, -5},
            { -5,  0,  0,  0,  0,  0,  0, -5},
            { 20, 25, 25, 25, 25, 25, 25, 20}, 
            {  0,  0,  5, 10, 10,  5,  0,  0}
        };
        private static readonly int[,] QueenTable = new int[8, 8]
        { 
            {-20,-10,-10, -5, -5,-10,-10,-20},
            {-10,  0,  5,  0,  0,  5,  0,-10},
            {-10,  0,  5,  5,  5,  5,  5,-10}, 
            { -5,  0,  5,  5,  5,  5,  0,  0},
            { -5,  0,  5,  5,  5,  5,  0, -5},
            {-10,  5,  5,  5,  5,  5,  0,-10},
            {-10,  0,  0,  0,  0,  0,  0,-10},
            {-20,-10,-10, -5, -5,-10,-10,-20}
        };
        
        private static readonly int[,] KingMidgameTable = new int[8, 8]
        {
            {-50, -50, -50, -50, -50, -50, -50, -50},
            {-50, -50, -50, -50, -50, -50, -50, -50},
            {-40, -40, -40, -40, -40, -40, -40, -40},
            {-30, -30, -30, -30, -30, -30, -30, -30},
            {-20, -20, -20, -20, -20, -20, -20, -20},
            {-10, -10, -10, -10, -10, -10, -10, -10},
            {  0,   5,   5, -10, -10,  5,   5,   0}, 
            { 10,  20,  20, -20, -20, 20,  20,  10}  
        };

        private static readonly int[,] KingEndgameTable = new int[8, 8]
        {
            {-50,-30,-20,-10,-10,-20,-30,-50},
            {-30,-10,  0, 10, 10,  0,-10,-30},
            {-20,  0, 20, 30, 30, 20,  0,-20},
            {-10, 10, 30, 40, 40, 30, 10,-10},
            {-10, 10, 30, 40, 40, 30, 10,-10},
            {-20,  0, 20, 30, 30, 20,  0,-20},
            {-30,-10,  0, 10, 10,  0,-10,-30},
            {-50,-30,-20,-10,-10,-20,-30,-50}
        };
        private const int OpeningMaterialThreshold = 2800; 
        private const int EndgameMaterialThreshold = 1500; 

        
        private const int BishopPairBonus = 50;
        private const int KnightPairPenalty = -10; 
        private const int RookOnOpenFileBonus = 15;
        private const int RookOnSemiOpenFileBonus = 10;
        private const int RookOn7thRankBonus = 25; 
        private const int DoubledRooksOn7thBonus = 60;
        private const int TempoBonus = 15; 
        private const int MobilityScaleFactor = 2; 

        public static int Evaluate(board board, Player perspective)
        {
            StateOfGame state = new StateOfGame(perspective, board); 
            if (state.IsGameOver())
            {
                return state.result.Reasonn switch
                {
                    EndReasonn.Checkmate => state.result.Winner == perspective ? CheckmateScore : -CheckmateScore,
                    EndReasonn.Stalemate => StalemateScore,
                    _ => DrawScore 
                };
            }

            int score = 0;
            int whiteMaterial = 0;
            int blackMaterial = 0;
            int[] whitePawnFiles = new int[8]; 
            int[] blackPawnFiles = new int[8]; 
            List<Position> whitePawns = new();
            List<Position> blackPawns = new();
            Position whiteKingPos = null, blackKingPos = null;
            int whiteKnightCount = 0, blackKnightCount = 0;
            int whiteBishopCount = 0, blackBishopCount = 0;
            List<Position> whiteRooks = new();
            List<Position> blackRooks = new();
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);
                    Piece piece = board[pos];
                    if (piece == null) continue;

                    int pieceVal = PieceValues[piece.Type];
                    if (piece.Color == Player.White) whiteMaterial += pieceVal;
                    else blackMaterial += pieceVal;

                    
                    int pstBonus = GetPieceSquareValue(piece, pos, whiteMaterial + blackMaterial); 
                    score += (piece.Color == perspective) ? (pieceVal + pstBonus) : -(pieceVal + pstBonus);

                    
                    if (piece.Type == PieceType.Pawn)
                    {
                        if (piece.Color == Player.White) { whitePawns.Add(pos); whitePawnFiles[c]++; }
                        else { blackPawns.Add(pos); blackPawnFiles[c]++; }
                    }
                    else if (piece.Type == PieceType.King)
                    {
                        if (piece.Color == Player.White) whiteKingPos = pos;
                        else blackKingPos = pos;
                    }
                    else if (piece.Type == PieceType.Knight)
                    {
                        if (piece.Color == Player.White) whiteKnightCount++; else blackKnightCount++;
                    }
                    else if (piece.Type == PieceType.Bishop)
                    {
                        if (piece.Color == Player.White) whiteBishopCount++; else blackBishopCount++;
                    }
                    else if (piece.Type == PieceType.Rook)
                    {
                        if (piece.Color == Player.White) whiteRooks.Add(pos); else blackRooks.Add(pos);
                    }
                }
            }

            int totalMaterialExcludingKings = whiteMaterial + blackMaterial - 2 * PieceValues[PieceType.King];

            
            score += EvaluatePawnStructure(whitePawnFiles, whitePawns, Player.White, board, perspective);
            score -= EvaluatePawnStructure(blackPawnFiles, blackPawns, Player.Black, board, perspective); 

            
            score += EvaluatePassedPawns(board, whitePawns, Player.White, perspective);
            score -= EvaluatePassedPawns(board, blackPawns, Player.Black, perspective);

            
            if (totalMaterialExcludingKings > EndgameMaterialThreshold) 
            {
                score += EvaluateKingSafety(whiteKingPos, Player.White, board, perspective);
                score -= EvaluateKingSafety(blackKingPos, Player.Black, board, perspective);
            }
            else 
            {
                
            }

            
            if (whiteBishopCount >= 2) score += (perspective == Player.White ? BishopPairBonus : -BishopPairBonus);
            if (blackBishopCount >= 2) score -= (perspective == Player.Black ? BishopPairBonus : -BishopPairBonus);
            if (whiteKnightCount >= 2) score += (perspective == Player.White ? KnightPairPenalty : -KnightPairPenalty);
            if (blackKnightCount >= 2) score -= (perspective == Player.Black ? KnightPairPenalty : -KnightPairPenalty);

            
            score += EvaluateRookPlacement(whiteRooks, Player.White, whitePawnFiles, blackPawnFiles, board, perspective);
            score -= EvaluateRookPlacement(blackRooks, Player.Black, blackPawnFiles, whitePawnFiles, board, perspective);
            int mobilityScore = state.AllLegalMovesFor(perspective).Count() * MobilityScaleFactor;
            score += mobilityScore;
            if (state.CurrentPlayer == perspective) 
            {
                score += TempoBonus;
            }
            else
            {
                score -= TempoBonus;
            }

            return score;
        }

        private static int GetPieceSquareValue(Piece piece, Position pos, int totalMaterial)
        {
            Player color = piece.Color;
            PieceType type = piece.Type;
            int r = (color == Player.White) ? pos.Row : 7 - pos.Row; 
            int c = pos.Column;

            if (type == PieceType.King)
            {
                int totalMaterialExKings = totalMaterial - 2 * PieceValues[PieceType.King];
                if (totalMaterialExKings <= EndgameMaterialThreshold) 
                    return KingEndgameTable[r, c];
                else 
                    return KingMidgameTable[r, c];
            }

            return type switch
            {
                PieceType.Pawn => PawnTable[r, c],
                PieceType.Knight => KnightTable[r, c],
                PieceType.Bishop => BishopTable[r, c],
                PieceType.Rook => RookTable[r, c],
                PieceType.Queen => QueenTable[r, c],
                _ => 0
            };
        }
        private static int EvaluatePawnStructure(int[] playerPawnFiles, List<Position> playerPawns, Player color, board board, Player perspective)
        {
            int structureScore = 0;
            const int doubledPawnPenalty = -15; 
            const int isolatedPawnPenalty = -20; 
            const int backwardPawnPenalty = -12;

            
            for (int c = 0; c < 8; c++)
            {
                if (playerPawnFiles[c] > 1)
                {
                    structureScore += doubledPawnPenalty * (playerPawnFiles[c] - 1);
                }
            }
            foreach (Position pawnPos in playerPawns)
            {
                int c = pawnPos.Column;
                bool leftSupport = (c > 0 && playerPawnFiles[c - 1] > 0);
                bool rightSupport = (c < 7 && playerPawnFiles[c + 1] > 0);

                if (!leftSupport && !rightSupport)
                {
                    structureScore += isolatedPawnPenalty;
                }
                bool canAdvance = true; 
                Position oneStepForward = pawnPos + (color == Player.White ? Direction.North : Direction.South);
                if (board.IsInside(0, oneStepForward) && board[oneStepForward] != null) canAdvance = false; 

                if (!canAdvance) 
                {
                    bool supportedLaterally = false;
                    int behindRank = color == Player.White ? pawnPos.Row + 1 : pawnPos.Row - 1;
                    if (c > 0 && behindRank >= 0 && behindRank < 8 && board[behindRank, c - 1]?.Type == PieceType.Pawn && board[behindRank, c - 1]?.Color == color) supportedLaterally = true;
                    if (c < 7 && behindRank >= 0 && behindRank < 8 && board[behindRank, c + 1]?.Type == PieceType.Pawn && board[behindRank, c + 1]?.Color == color) supportedLaterally = true;
                    if (!supportedLaterally && playerPawnFiles[c] == 1) 
                    {
                        structureScore += backwardPawnPenalty;
                    }
                }
            }
            return (color == perspective) ? structureScore : -structureScore;
        }
        private static int EvaluatePassedPawns(board board, List<Position> pawns, Player color, Player perspective)
        {
            int bonus = 0;
            foreach (var pawnPos in pawns)
            {
                bool isPassed = true;
                
                for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
                {
                    int checkFile = pawnPos.Column + fileOffset;
                    if (checkFile < 0 || checkFile > 7) continue;

                    int currentRank = pawnPos.Row;
                    if (color == Player.White) 
                    {
                        for (int r = currentRank - 1; r >= 0; r--)
                        {
                            Piece p = board[r, checkFile];
                            if (p != null && p.Type == PieceType.Pawn && p.Color == Player.Black)
                            {
                                isPassed = false; break;
                            }
                        }
                    }
                    else 
                    {
                        for (int r = currentRank + 1; r <= 7; r++)
                        {
                            Piece p = board[r, checkFile];
                            if (p != null && p.Type == PieceType.Pawn && p.Color == Player.White)
                            {
                                isPassed = false; break;
                            }
                        }
                    }
                    if (!isPassed) break;
                }

                if (isPassed)
                {
                    int rank = (color == Player.White) ? (7 - pawnPos.Row) : pawnPos.Row; 
                    bonus += 15 + (rank * rank * 2); 
                                                     
                }
            }
            return (color == perspective) ? bonus : -bonus;
        }

        private static int EvaluateKingSafety(Position kingPos, Player kingColor, board board, Player perspective)
        {
            if (kingPos == null) return 0;
            int safetyScore = 0;

            
            int pawnShieldCount = 0;
            int shieldRow = kingColor == Player.White ? kingPos.Row - 1 : kingPos.Row + 1;
            int[] shieldFiles = { kingPos.Column - 1, kingPos.Column, kingPos.Column + 1 };

            foreach (int file in shieldFiles)
            {
                if (file < 0 || file > 7) continue;
                Position shieldPos = new Position(shieldRow, file);
                if (board.IsInside(0, shieldPos) &&
                    board[shieldPos]?.Type == PieceType.Pawn &&
                    board[shieldPos]?.Color == kingColor)
                {
                    pawnShieldCount++;
                }
            }
            safetyScore += pawnShieldCount * 15; 

            
            bool kingFileOpen = true;
            for (int r = 0; r < 8; r++)
            {
                if (board[r, kingPos.Column]?.Type == PieceType.Pawn)
                {
                    kingFileOpen = false;
                    break;
                }
            }
            if (kingFileOpen) safetyScore -= 25;

            
            int[] adjacentFiles = { kingPos.Column - 1, kingPos.Column + 1 };
            foreach (int file in adjacentFiles)
            {
                if (file < 0 || file > 7) continue;

                bool hasFriendlyPawn = false;
                for (int r = 0; r < 8; r++)
                {
                    Piece p = board[r, file];
                    if (p?.Type == PieceType.Pawn && p.Color == kingColor)
                    {
                        hasFriendlyPawn = true;
                        break;
                    }
                }
                if (!hasFriendlyPawn) safetyScore -= 15;
            }

            
            int centerDistance = Math.Abs(3 - kingPos.Column) + Math.Abs(3 - kingPos.Row);
            safetyScore -= centerDistance * 5; 

            return (kingColor == perspective) ? safetyScore : -safetyScore;
        }
        private static int EvaluateRookPlacement(List<Position> rooks, Player color, int[] friendlyPawnFiles, int[] opponentPawnFiles, board board, Player perspective)
        {
            int rookScore = 0;
            if (rooks.Count == 0) return 0;

            foreach (var rookPos in rooks)
            {
                
                if ((color == Player.White && rookPos.Row == 1) || (color == Player.Black && rookPos.Row == 6)) 
                {
                    rookScore += RookOn7thRankBonus;
                }

                
                bool fileIsOpen = true;
                bool fileIsSemiOpen = true;

                for (int r = 0; r < 8; r++)
                {
                    if (friendlyPawnFiles[rookPos.Column] > 0) fileIsSemiOpen = false; 
                    if (opponentPawnFiles[rookPos.Column] > 0) fileIsOpen = false;     
                }

                if (fileIsOpen && fileIsSemiOpen) rookScore += RookOnOpenFileBonus; 
                else if (fileIsSemiOpen) rookScore += RookOnSemiOpenFileBonus; 
            }

            
            if (rooks.Count == 2)
            {
                bool firstRookOn7th = (color == Player.White && rooks[0].Row == 1) || (color == Player.Black && rooks[0].Row == 6);
                bool secondRookOn7th = (color == Player.White && rooks[1].Row == 1) || (color == Player.Black && rooks[1].Row == 6);
                if (firstRookOn7th && secondRookOn7th && rooks[0].Column == rooks[1].Column) 
                {
                    rookScore += DoubledRooksOn7thBonus - (2 * RookOn7thRankBonus); 
                }
            }


            return (color == perspective) ? rookScore : -rookScore;
        }
    }
}
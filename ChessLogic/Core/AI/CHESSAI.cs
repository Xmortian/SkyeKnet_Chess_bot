using ChessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ChessLogic.Core.Ai; 

public class ChessAI
{
    private readonly int maxSearchDepth;
    private readonly Player aiPlayer;
    private readonly List<string> moveHistory = new();
    
    
    private readonly int timeLimitMs = 5000;
    private static Stopwatch turnStopwatch;

    private DateTime lastOpponentMoveTime = DateTime.UtcNow;
    private Move lastOpponentMove; 

    public ChessAI(Player aiPlayer, int depth = 15)
    {
        this.aiPlayer = aiPlayer;
        this.maxSearchDepth = depth;
    }

    private bool IsCapture(Move move, board board)
    {
        
        
        if (move.ToPos == null || !(move.ToPos.Row >= 0 && move.ToPos.Row < 8 && move.ToPos.Column >= 0 && move.ToPos.Column < 8))
        {
            return false;
        }
        return board[move.ToPos] != null;
    }

    public Move GetBestMove(StateOfGame gameState)
    {
        
        
        

        turnStopwatch = Stopwatch.StartNew();
        Move bestMoveOverall = null;
        int bestScoreOverall = int.MinValue;


        string bookMoveUci = OpeningBook.GetBookMove(moveHistory);
        if (bookMoveUci != null)
        {
            var legalMovesFromBook = gameState.AllLegalMovesFor(gameState.CurrentPlayer).ToList();
            var matchingMove = legalMovesFromBook.FirstOrDefault(move => move.ToUCI() == bookMoveUci);
            if (matchingMove != null)
            {
                moveHistory.Add(bookMoveUci);
                turnStopwatch.Stop();
                Console.WriteLine($"Bot plays book move: {matchingMove.ToUCI()}");
                return matchingMove;
            }

            foreach (var move in legalMovesFromBook)
            {
                if (move.ToUCI().StartsWith(bookMoveUci.Substring(0, Math.Min(bookMoveUci.Length, 4))))
                {
                    moveHistory.Add(move.ToUCI());
                    turnStopwatch.Stop();
                    Console.WriteLine($"Bot plays book move (partial match): {move.ToUCI()}");
                    return move;
                }
            }
        }

        Console.WriteLine($"AI ({aiPlayer}) searching with max depth: {maxSearchDepth}");

        for (int currentDepth = 1; currentDepth <= maxSearchDepth; currentDepth++)
        {
            if (StopwatchElapsed())
            {
                Console.WriteLine($"Time limit reached before starting depth {currentDepth}. Using best move from depth {currentDepth - 1}.");
                break;
            }
            Console.WriteLine($"Searching depth: {currentDepth}");

            int bestScoreThisIteration = int.MinValue;
            Move bestMoveThisIteration = null;

            
            var moves = MoveOrdering.OrderMoves(gameState.AllLegalMovesFor(aiPlayer), gameState.Board, currentDepth).ToList();
            if (!moves.Any()) break;

            if (bestMoveOverall == null && currentDepth == 1)
            {
                bestMoveOverall = moves.First();
                var tempBoard = gameState.Board.Copy();
                bestMoveOverall.Execute(tempBoard);
                bestScoreOverall = Evaluator.Evaluate(tempBoard, aiPlayer);
            }

            foreach (Move move in moves)
            {
                if (StopwatchElapsed())
                {
                    Console.WriteLine($"Time limit reached during depth {currentDepth}.");
                    goto EndSearch;
                }

                var copy = gameState.Board.Copy();
                if (move.IsLegal(copy))  
                {
                    move.Execute(copy);
                    int score = Minimax(copy, currentDepth - 1, int.MinValue, int.MaxValue, false, 0);
                    if (score > bestScoreThisIteration)
                    {
                        bestScoreThisIteration = score;
                        bestMoveThisIteration = move;
                    }

                }
                


            }

            if (bestMoveThisIteration != null && (!StopwatchElapsed() || bestMoveOverall == null))
            {
                bestMoveOverall = bestMoveThisIteration;
                bestScoreOverall = bestScoreThisIteration;
                Console.WriteLine($"Depth {currentDepth} completed. Best move so far: {bestMoveOverall.ToUCI()} (Score: {bestScoreOverall})");
            }
            else if (StopwatchElapsed() && bestMoveThisIteration == null && bestMoveOverall == null && moves.Any())
            {
                bestMoveOverall = moves.First();
                var tempBoard = gameState.Board.Copy();
                bestMoveOverall.Execute(tempBoard);
                bestScoreOverall = Evaluator.Evaluate(tempBoard, aiPlayer);
                Console.WriteLine($"Time out very early, fallback to first legal move: {bestMoveOverall.ToUCI()} with score {bestScoreOverall}");
            }
        }

    EndSearch:
        turnStopwatch.Stop();
        Console.WriteLine($"Search took: {turnStopwatch.ElapsedMilliseconds}ms");

        if (bestMoveOverall == null)
        {
            var allMoves = gameState.AllLegalMovesFor(aiPlayer).ToList();
            if (allMoves.Any())
            {
                bestMoveOverall = allMoves.First();
                var tempBoard = gameState.Board.Copy();
                bestMoveOverall.Execute(tempBoard);
                bestScoreOverall = Evaluator.Evaluate(tempBoard, aiPlayer);
                Console.WriteLine($"Search failed to find a move or timed out completely. Fallback to first legal move: {bestMoveOverall.ToUCI()} with score {bestScoreOverall}");
            }
        }

        if (bestMoveOverall != null)
        {
            moveHistory.Add(bestMoveOverall.ToUCI());
            if (bestScoreOverall == int.MinValue) 
            {
                var tempBoardEval = gameState.Board.Copy();
                bestMoveOverall.Execute(tempBoardEval);
                bestScoreOverall = Evaluator.Evaluate(tempBoardEval, aiPlayer);
            }

            
            string botLine = BotDialogue.GetBotLine(
                phase: GetPhase(gameState.Board),
                eval: bestScoreOverall,
                opponentTookLong: OpponentTookTooLong(),
                lastOpponentMove: GetLastOpponentMove() ?? bestMoveOverall, 
                currentBoard: gameState.Board, 
                aiPlayer: this.aiPlayer,       
                isWin: gameState.IsGameOver() && gameState.result != null && gameState.result.Winner == aiPlayer,
                isLoss: gameState.IsGameOver() && gameState.result != null && gameState.result.Winner == aiPlayer.Opponent()
            );
            if (!string.IsNullOrWhiteSpace(botLine))
                Console.WriteLine($"Bot says: \"{botLine}\"");
        }
        else
        {
            Console.WriteLine("AI could not find a legal move (e.g., stalemate/checkmate already processed or error).");
        }

        return bestMoveOverall;
    }

    private int Minimax(board board, int depth, int alpha, int beta, bool maximizing, int currentPlyFromRoot)
    {
        if (StopwatchElapsed()) 
            return Evaluator.Evaluate(board, aiPlayer); 

        Player currentPlayerToEvaluate = maximizing ? aiPlayer : aiPlayer.Opponent();
        StateOfGame state = new StateOfGame(currentPlayerToEvaluate, board);

        ulong hash = ZobristHasher.ComputeHash(board, currentPlayerToEvaluate);
        int alphaOrig = alpha;

        if (TranspositionTable.TryGet(hash, depth, alpha, beta, out int ttScore))
        {
            return ttScore;
        }

        var legalMoves = state.AllLegalMovesFor(currentPlayerToEvaluate).ToList();

        if (depth == 0 || !legalMoves.Any())
        {
            if (!legalMoves.Any()) 
            {
                if (state.Board.IsInCheck(currentPlayerToEvaluate)) 
                    return maximizing ? (-CheckmateScore + currentPlyFromRoot) : (CheckmateScore - currentPlyFromRoot);
                else 
                    return 0;
            }
            
            int quietScore = QuiescenceSearcher.Quiescence(state, alpha, beta, currentPlayerToEvaluate);
            TranspositionTable.Store(hash, depth, quietScore, NodeType.Exact);
            return quietScore;
        }

        int bestEval = maximizing ? int.MinValue : int.MaxValue;
        var orderedMoves = MoveOrdering.OrderMoves(legalMoves, board, currentPlyFromRoot);

        foreach (var move in orderedMoves)
        {
            if (StopwatchElapsed()) break; 

            var copy = board.Copy();
            move.Execute(copy);

            int eval = Minimax(copy, depth - 1, alpha, beta, !maximizing, currentPlyFromRoot + 1);

            if (maximizing)
            {
                if (eval > bestEval) bestEval = eval;
                if (eval > alpha)
                {
                    alpha = eval;
                    MoveOrdering.AddHistoryBonus(move, depth);
                }
            }
            else 
            {
                if (eval < bestEval) bestEval = eval;
                if (eval < beta)
                {
                    beta = eval;
                    MoveOrdering.AddHistoryBonus(move, depth);
                }
            }

            if (beta <= alpha) 
            {
                if (!IsCapture(move, board)) 
                {
                    MoveOrdering.StoreKillerMove(move, currentPlyFromRoot);
                }
                break;
            }
        }

        NodeType nodeType = NodeType.Exact;
        if (bestEval <= alphaOrig) nodeType = NodeType.UpperBound;
        else if (bestEval >= beta) nodeType = NodeType.LowerBound;

        TranspositionTable.Store(hash, depth, bestEval, nodeType);
        return bestEval;
    }

    private const int CheckmateScore = 99999;

    public static bool StopwatchElapsed() 
    {

        return turnStopwatch != null && turnStopwatch.ElapsedMilliseconds >= 8000;
    }

    private GamePhase GetPhase(board board)
    {
        int totalMaterialValue = 0;
        foreach (Position pos in board.PiecePositions())
        {
            Piece piece = board[pos];
            if (piece != null && piece.Type != PieceType.King)
            {
                totalMaterialValue += Evaluator.GetPieceValue(piece.Type);
            }
        }
        if (totalMaterialValue > 4000) return GamePhase.Opening;
        if (totalMaterialValue > 1800) return GamePhase.Middlegame;
        return GamePhase.Endgame;
    }

    public void NotifyOpponentMoved()
    {
        lastOpponentMoveTime = DateTime.UtcNow;
    }

    private bool OpponentTookTooLong()
    {
        return (DateTime.UtcNow - lastOpponentMoveTime).TotalMilliseconds > 45000;
    }

    public void SetLastOpponentMove(Move move)
    {
        lastOpponentMove = move;
    }

    private Move GetLastOpponentMove()
    {
        return lastOpponentMove;
    }
}

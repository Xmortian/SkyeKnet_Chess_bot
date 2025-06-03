
using ChessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ChessLogic.Core.Ai;

public class ChessAI
{
    public readonly int maxSearchDepth;
    public readonly Player aiPlayer;
    private readonly List<string> moveHistory = new();
    private int nodesSearched = 0;


    private bool gameEnded = false;

    private readonly int timeLimitMs = 5000;
    private static Stopwatch turnStopwatch;

    private DateTime lastOpponentMoveTime = DateTime.UtcNow;
    private Move lastOpponentMove;

    public ChessAI(Player aiPlayer, int depth = 12)
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

        if (gameEnded || gameState.IsGameOver())
        {
            //Debug.WriteLine("[AI] Game ended - no move generated");
            return null;
        }



        turnStopwatch = Stopwatch.StartNew();
        Move bestMoveOverall = null;
        int bestScoreOverall = int.MinValue;


        string bookMoveSuggestionUci = OpeningBook.GetBookMove(gameState.MoveHistoryUCI);

        if (bookMoveSuggestionUci != null)
        {
            //Debug.WriteLine($"[AI BookCheck] Book suggested: {bookMoveSuggestionUci}");
            var legalMovesForCurrentAIPlayer = gameState.AllLegalMovesFor(gameState.CurrentPlayer).ToList();
            Move moveToPlay = null;

            // 2. Attempt to find an EXACT match for the book move among all legal moves.
            moveToPlay = legalMovesForCurrentAIPlayer.FirstOrDefault(legalMove => legalMove.ToUCI() == bookMoveSuggestionUci);

            if (moveToPlay != null)
            {
                //Debug.WriteLine($"[AI BookCheck] Exact book move found and is legal: {moveToPlay.ToUCI()}");
            }
            else
            {
                //Debug.WriteLine($"[AI BookCheck] Exact book move '{bookMoveSuggestionUci}' not found or not legal. Trying partial match...");

                string bookMoveBase = bookMoveSuggestionUci.Substring(0, Math.Min(bookMoveSuggestionUci.Length, 4));
                moveToPlay = legalMovesForCurrentAIPlayer.FirstOrDefault(legalMove => legalMove.ToUCI().StartsWith(bookMoveBase));

                if (moveToPlay != null)
                {
                    //Debug.WriteLine($"[AI BookCheck] Partial book move match found and is legal: {moveToPlay.ToUCI()}");
                }
            }

            // 4. If a playable move (either exact or partial) was found:
            if (moveToPlay != null)
            {
                //Debug.WriteLine($"[AI Plays BookMove] Playing: {moveToPlay.ToUCI()}");



                return moveToPlay;    // Return the chosen book move.
            }
            else
            {
                //Debug.WriteLine($"[AI BookCheck] Book suggested '{bookMoveSuggestionUci}', but no matching legal move found. Proceeding to search.");
            }
        }


        //Debug.WriteLine($"AI ({aiPlayer}) searching with max depth: {maxSearchDepth}");

        for (int currentDepth = 1; currentDepth <= maxSearchDepth; currentDepth++)
        {
            if (StopwatchElapsed())
            {
                //Debug.WriteLine($"Time limit reached before starting depth {currentDepth}. Using best move from depth {currentDepth - 1}.");
                break;
            }
            //Debug.WriteLine($"Searching depth: {currentDepth}");

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
                    //Debug.WriteLine($"Time limit reached during depth {currentDepth}.");
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
                Debug.WriteLine($"Depth {currentDepth} completed. Best move so far: {bestMoveOverall.ToUCI()} (Score: {bestScoreOverall})");
            }
            else if (StopwatchElapsed() && bestMoveThisIteration == null && bestMoveOverall == null && moves.Any())
            {
                bestMoveOverall = moves.First();
                var tempBoard = gameState.Board.Copy();
                bestMoveOverall.Execute(tempBoard);
                bestScoreOverall = Evaluator.Evaluate(tempBoard, aiPlayer);
                //Debug.WriteLine($"Time out very early, fallback to first legal move: {bestMoveOverall.ToUCI()} with score {bestScoreOverall}");
            }
        }

    EndSearch:
        turnStopwatch.Stop();
        Debug.WriteLine($"Search took: {turnStopwatch.ElapsedMilliseconds}ms");

        if (bestMoveOverall == null)
        {
            var allMoves = gameState.AllLegalMovesFor(aiPlayer).ToList();
            if (allMoves.Any())
            {
                bestMoveOverall = allMoves.First();
                var tempBoard = gameState.Board.Copy();
                bestMoveOverall.Execute(tempBoard);
                bestScoreOverall = Evaluator.Evaluate(tempBoard, aiPlayer);
                Debug.WriteLine($"Search failed to find a move or timed out completely. Fallback to first legal move: {bestMoveOverall.ToUCI()} with score {bestScoreOverall}");
            }
        }

        if (bestMoveOverall != null)
        {
            moveHistory.Add(bestMoveOverall.ToUCI());
            //Debug.WriteLine($"[AI Move] Chose normal move: {bestMoveOverall.ToUCI()}");
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
                isWin: false,  
                isLoss: false  
            );
            if (!string.IsNullOrWhiteSpace(botLine)) ;
                //Debug.WriteLine($"Bot says: \"{botLine}\"");
        }
        else
        {
            //Debug.WriteLine("AI could not find a legal move (e.g., stalemate/checkmate already processed or error).");
        }
        Debug.WriteLine($"[Search Complete] Nodes searched: {nodesSearched}");
        nodesSearched = 0; // Reset for next move

        return bestMoveOverall;
    }

    private int Minimax(board board, int depth, int alpha, int beta, bool maximizing, int currentPlyFromRoot)
    {
        Player currentPlayerToEvaluate = maximizing ? aiPlayer : aiPlayer.Opponent();
        // Faster creation without triggering StateString
        StateOfGame state = new StateOfGame(currentPlayerToEvaluate, board, false);
        var legalMoves = state.AllLegalMovesFor(currentPlayerToEvaluate).ToList();
        if (StopwatchElapsed())
        {
            //Debug.WriteLine($"[Time Check] Timeout at depth={depth}, ply={currentPlyFromRoot}, nodes={nodesSearched}");
            return Evaluator.Evaluate(board, aiPlayer, legalMoves);
        }

        nodesSearched++; // ✅ Count each node evaluated


        ulong hash = ZobristHasher.ComputeHash(board, currentPlayerToEvaluate);
        int alphaOrig = alpha;

        if (TranspositionTable.TryGet(hash, depth, alpha, beta, out int ttScore))
        {
            return ttScore;
        }


        if (!legalMoves.Any())
        {
            // Game over due to checkmate or stalemate
            if (board.IsInCheck(currentPlayerToEvaluate))
                return maximizing ? (-CheckmateScore + currentPlyFromRoot) : (CheckmateScore - currentPlyFromRoot);
            else
                return 0; // Stalemate
        }
        else if (depth == 0)
        {
            int quietScore = QuiescenceSearcher.Quiescence(
                new StateOfGame(currentPlayerToEvaluate, board), alpha, beta, currentPlayerToEvaluate
            );
            TranspositionTable.Store(hash, depth, quietScore, NodeType.Exact);
            return quietScore;
        }

        int bestEval = maximizing ? int.MinValue : int.MaxValue;
        var orderedMoves = MoveOrdering.OrderMoves(legalMoves, board, currentPlyFromRoot);

        if (orderedMoves.Count > 25 && depth >= 3)
        {
            orderedMoves = orderedMoves.Take(15).ToList();
        }


        int moveIndex = 0;

        foreach (var move in orderedMoves)
        {
            if (StopwatchElapsed()) break;

            var tempBoard = board.Copy();
            move.Execute(tempBoard);

            int searchDepth = depth - 1;

            bool isQuiet = !IsCapture(move, board);
            if (moveIndex >= 3 && depth >= 3 && isQuiet)
            {
                searchDepth -= 1;
            }

            int eval = Minimax(tempBoard, searchDepth, alpha, beta, !maximizing, currentPlyFromRoot + 1);

            if (searchDepth < depth - 1 && ((maximizing && eval > alpha) || (!maximizing && eval < beta)))
            {
                eval = Minimax(tempBoard, depth - 1, alpha, beta, !maximizing, currentPlyFromRoot + 1);
            }

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
                if (isQuiet)
                    MoveOrdering.StoreKillerMove(move, currentPlyFromRoot);
                break;
            }

            moveIndex++;
        }


        NodeType nodeType = NodeType.Exact;
        if (bestEval <= alphaOrig) nodeType = NodeType.UpperBound;
        else if (bestEval >= beta) nodeType = NodeType.LowerBound;

        TranspositionTable.Store(hash, depth, bestEval, nodeType);
        //Debug.WriteLine($"[Depth={depth}, Ply={currentPlyFromRoot}] Nodes so far: {nodesSearched}"); I will come back to it later on

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
    public void NotifyGameEnded(bool aiWon)
    {
        gameEnded = true;
        moveHistory.Clear();
        lastOpponentMove = null;
        lastOpponentMoveTime = DateTime.UtcNow;
    }

    public void ResetForNewGame()
    {
        gameEnded = false;
        moveHistory.Clear();
        lastOpponentMove = null;
        lastOpponentMoveTime = DateTime.UtcNow;
        nodesSearched = 0;
    }
}

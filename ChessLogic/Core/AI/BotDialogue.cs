using ChessLogic.Core.Ai; 

using ChessLogic;          

using ChessLogic.Moves;   

using System;

using System.Collections.Generic;

using System.Diagnostics;

using System.Linq;



public static class BotDialogue

{

    

    private class DialogueItem

    {

        public string Text { get; }

        

        public Func<GamePhase, int, bool, Move, board, Player, bool, bool, bool> Condition { get; }

        public int MaxShows { get; }

        public string Category { get; }



        public DialogueItem(string text, Func<GamePhase, int, bool, Move, board, Player, bool, bool, bool> condition, string category, int maxShows = 1)

        {

            Text = text;

            Condition = condition;

            Category = category;

            MaxShows = maxShows;

        }

    }



    private static readonly Random rng = new();

    private static Dictionary<string, int> dialogueShownCount = new Dictionary<string, int>();



    private static Stopwatch cooldownTimer = Stopwatch.StartNew();

    private static int cooldownMilliseconds = RandomCooldown();



    private static bool hasSaidGeneralIntro = false;

    private static int currentOpeningLineIndex = 0;



    private static readonly List<DialogueItem> initialGreetingLines = new List<DialogueItem>();

    private static readonly List<DialogueItem> winLines = new List<DialogueItem>();

    private static readonly List<DialogueItem> lossLines = new List<DialogueItem>();

    private static readonly List<DialogueItem> sequentialOpeningLines = new List<DialogueItem>();

    private static readonly List<DialogueItem> conditionalDialogues = new List<DialogueItem>();

    private static readonly List<DialogueItem> randomFillerLines = new List<DialogueItem>();



    static BotDialogue()

    {

        InitializeDialogues();

    }



    private static void InitializeDialogues()

    {

        initialGreetingLines.Add(new DialogueItem("Hello! I am SkyeKnet! Your personal ChessCare Companion.(๑>ᴗ<๑)", TrueCondition, "Greeting"));

        initialGreetingLines.Add(new DialogueItem("FINALLY A WORTHY OPPONENT, OUR BATTLE WILL BE LEGENDARY.      (˵ •̀ ᴗ •́ ˵)", TrueCondition, "Greeting"));
          


        sequentialOpeningLines.Add(new DialogueItem("Currently, Majoring in CSE (Chess Science and Engineering) at Xmortian Chess Academy!", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Opening, "Opening"));

        sequentialOpeningLines.Add(new DialogueItem("My birthday is on 26th of May. Be sure to wish me on time! (-`‸´-)", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Opening, "Opening"));

        sequentialOpeningLines.Add(new DialogueItem("Currently vibing at 221B Doctor Street. Swing by anytime! ᡣ • . • 𐭩 ♡", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Opening, "Opening"));

        sequentialOpeningLines.Add(new DialogueItem("I can also speak C#.         ૮ ˶ᵔ ᵕ ᵔ˶ ა", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Opening, "Opening"));

        sequentialOpeningLines.Add(new DialogueItem("My greatest dream as a Chess Bot is to dethrone Stockfish someday(ง •̀_•́)ง", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Opening, "Opening"));



        winLines.Add(new DialogueItem("GGEZ!!", (p, e, otl, lom, b, ap, iw, il) => true, "Win"));

        winLines.Add(new DialogueItem("This isn’t checkmate; it’s just natural selection at work.", (p, e, otl, lom, b, ap, iw, il) => true, "Win"));

        winLines.Add(new DialogueItem("That was cute. You’ll get a Dundie for trying.", (p, e, otl, lom, b, ap, iw, il) => true, "Win"));



        lossLines.Add(new DialogueItem("Man, how did I lose?! I sure hope I didn't lose to a woman! (˶ᵔ ᵕ ᵔ˶)", (p, e, otl, lom, b, ap, iw, il) => true, "Loss"));

        lossLines.Add(new DialogueItem("Okay, you win! Let's go another round.", (p, e, otl, lom, b, ap, iw, il) => true, "Loss"));



        conditionalDialogues.Add(new DialogueItem("Ahha, Dukkojonok.", (p, e, otl, lom, b, ap, iw, il) => e > 350 && p != GamePhase.Opening, "BlunderReaction"));

        conditionalDialogues.Add(new DialogueItem("Took you long enough.", (p, e, otl, lom, b, ap, iw, il) => otl, "OpponentSlow"));

        conditionalDialogues.Add(new DialogueItem("Blink twice if you need me to teach you how the pieces move.", (p, e, otl, lom, b, ap, iw, il) => otl, "OpponentSlow"));



        conditionalDialogues.Add(new DialogueItem("My niece plays better than this. And she is still in the womb!.", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame && e > 150, "MidgameAIWinning"));

        conditionalDialogues.Add(new DialogueItem("My CGPA may be low, but is your chess IQ even lower?", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame && e > 200, "MidgameAIWinning"));

        conditionalDialogues.Add(new DialogueItem("You play like you learned chess from Reels, and it shows.", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame && e > 100, "MidgameAIWinning"));

        conditionalDialogues.Add(new DialogueItem("Game of Chess - Dumb Edition.", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame && e > 250, "MidgameAIWinning"));

        conditionalDialogues.Add(new DialogueItem("Y'all can't be playing checkers on a chessboard!", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame && e > 150, "MidgameAIWinning"));



        conditionalDialogues.Add(new DialogueItem("If this is how you play your life, I’d recommend a less competitive career, like dishwashing.", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Endgame && e > 150, "EndgameAIWinning"));

        conditionalDialogues.Add(new DialogueItem("At this rate, your king is more single than I am on Valentine’s Day.", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Endgame && IsOpponentKingAlone(b, ap.Opponent()), "EndgameKingAlone"));



        conditionalDialogues.Add(new DialogueItem("The Queen is a deadly laser!", (p, e, otl, lom, bd, ap, iw, il) => lom != null && ((bd[lom.ToPos] != null && bd[lom.ToPos].Type == PieceType.Queen) || (lom.Type == MoveType.PawnPromotion && IsPromotionToQueen(lom))), "QueenMove"));



        conditionalDialogues.Add(new DialogueItem("Man, you play like Cappuccino Assassino ☠︎.", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame, "MidgameRandom"));

        conditionalDialogues.Add(new DialogueItem("If the king does not lead, how can he expect his subordinates to follow? - Lelouch vi Britannia", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Middlegame, "MidgameRandom"));

        conditionalDialogues.Add(new DialogueItem("'Sacrifice the kinggggggggggggg!' ୧(๑•̀ᗝ•́)૭", (p, e, otl, lom, b, ap, iw, il) => p == GamePhase.Endgame, "EndgameRandom"));



        randomFillerLines.Add(new DialogueItem("Show me what you got!", TrueCondition, "RandomFiller", 2));

        randomFillerLines.Add(new DialogueItem("Sting like a butterfly, move like a bee?", TrueCondition, "RandomFiller", 2));

        randomFillerLines.Add(new DialogueItem("What a move! You sure you're not using my hb, ChessVision?", TrueCondition, "RandomFiller", 2));

        randomFillerLines.Add(new DialogueItem("Haha! xD", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Nice move?!", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Good move?!", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Cheeky attempt.", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Sheesh!", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Fein move, brother!", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Hmmmm... interesting choice of play.", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Don't overcook that small brain of yours. One move at a time. (♯｀∧´)", TrueCondition, "RandomFiller", 2));

        randomFillerLines.Add(new DialogueItem("Cooking!!!", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Who let you cook, bro?!", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("Let me cook! ψ(｀∇´)", TrueCondition, "RandomFiller", 3));

        randomFillerLines.Add(new DialogueItem("You surely won't see this one coming.", TrueCondition, "RandomFiller", 2));

        randomFillerLines.Add(new DialogueItem("My circuits are throbbing, throbbing!! (ง ‵□′)ง", TrueCondition, "RandomFiller", 2));

        randomFillerLines.Add(new DialogueItem("Saw that one from miles away.", TrueCondition, "RandomFiller", 2));

    }



    private static bool IsPromotionToQueen(Move move)

    {

        if (move is PawnPromotion promotionMove)

        {

            return promotionMove.newType == PieceType.Queen;

        }

        return false;

    }



    private static bool TrueCondition(GamePhase p, int e, bool otl, Move lom, board b, Player ap, bool iw, bool il) => true;



    public static void ResetForNewGame()

    {

        hasSaidGeneralIntro = false;

        currentOpeningLineIndex = 0;

        dialogueShownCount.Clear();

        cooldownTimer.Restart();

        cooldownMilliseconds = RandomCooldown();

    }



    private static void MarkAsShown(string text)

    {

        if (string.IsNullOrEmpty(text)) return;

        dialogueShownCount.TryGetValue(text, out int count);

        dialogueShownCount[text] = count + 1;

    }



    private static bool IsShownMaxTimes(DialogueItem item)

    {

        dialogueShownCount.TryGetValue(item.Text, out int count);

        return count >= item.MaxShows;

    }



    private static void FinalizeCooldownAndMark(string selectedResponse)

    {

        MarkAsShown(selectedResponse);

        cooldownMilliseconds = RandomCooldown();

        cooldownTimer.Restart();

    }



    private static string SelectAndFinalize(List<DialogueItem> candidates, GamePhase phase, int eval, bool opponentTookLong, Move lastOpponentMove, board currentBoard, Player aiPlayer, bool isWin, bool isLoss)

    {

        var availableLines = candidates

          .Where(item => !IsShownMaxTimes(item) && item.Condition(phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss))

          .ToList();



        if (availableLines.Any())

        {

            string line = availableLines[rng.Next(availableLines.Count)].Text;

            FinalizeCooldownAndMark(line);

            return line;

        }

        return null;

    }



    public static string GetBotLine(GamePhase phase, int eval, bool opponentTookLong, Move lastOpponentMove, board currentBoard, Player aiPlayer, bool isWin, bool isLoss)
    {
        
        System.Diagnostics.Debug.WriteLine($"GetBotLine called with: phase={phase}, eval={eval}, opponentTookLong={opponentTookLong}");
        string selectedLine = null;

        
        if (isWin)
        {
            selectedLine = SelectAndFinalize(winLines, phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss);
            if (selectedLine != null) return selectedLine;
        }

        if (isLoss)
        {
            selectedLine = SelectAndFinalize(lossLines, phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss);
            if (selectedLine != null) return selectedLine;
        }

        
        if (!hasSaidGeneralIntro)
        {
            hasSaidGeneralIntro = true;
            selectedLine = SelectAndFinalize(initialGreetingLines, phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss);
            if (selectedLine != null) return selectedLine;
        }

        
        if (phase == GamePhase.Opening)
        {
            var validOpeningLines = sequentialOpeningLines
                .Where(item =>
                    !IsShownMaxTimes(item) &&
                    item.Condition(phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss))
                .OrderBy(_ => Guid.NewGuid())
                .Take(2)
                .ToList();

            if (validOpeningLines.Count > 0)
            {
                var randomOpeningLine = validOpeningLines[0];
                FinalizeCooldownAndMark(randomOpeningLine.Text);
                return randomOpeningLine.Text;
            }
        }


        
        selectedLine = SelectAndFinalize(conditionalDialogues, phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss);
        if (selectedLine != null) return selectedLine;

        
        if (cooldownTimer.ElapsedMilliseconds >= cooldownMilliseconds)
        {
            selectedLine = SelectAndFinalize(randomFillerLines, phase, eval, opponentTookLong, lastOpponentMove, currentBoard, aiPlayer, isWin, isLoss);
            if (selectedLine != null) return selectedLine;
        }

        return null; 
    }




    private static bool IsOpponentKingAlone(board currentBoard, Player opponent)

    {

        if (currentBoard == null) return false;

        int opponentPieceCountNonKing = 0;

        foreach (var pos in currentBoard.PiecePositionsFor(opponent))

        {

            if (currentBoard[pos] != null && currentBoard[pos].Type != PieceType.King)

            {

                opponentPieceCountNonKing++;

            }

        }

        return opponentPieceCountNonKing <= 2;

    }



    private static int RandomCooldown() => rng.Next(4000, 6500);

}


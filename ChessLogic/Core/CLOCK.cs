using ChessLogic;
using System;

public class ChessClock
{
    public TimeSpan WhiteTime { get; private set; }
    public TimeSpan BlackTime { get; private set; }

    private DateTime? turnStartTime;
    private Player currentPlayer;
    private bool isGameOver = false;

    public bool IsRunning => turnStartTime != null && !isGameOver;

    public ChessClock()
    {
        WhiteTime = TimeSpan.FromMinutes(10);
        BlackTime = TimeSpan.FromMinutes(10);
    }

    public void StartTurn(Player player)
    {
        if (isGameOver) return;

        currentPlayer = player;
        turnStartTime = DateTime.Now;
    }

    public void EndTurn()
    {
        if (isGameOver || turnStartTime == null) return;

        TimeSpan elapsed = DateTime.Now - turnStartTime.Value;

        if (currentPlayer == Player.White)
            WhiteTime -= elapsed;
        else
            BlackTime -= elapsed;

        turnStartTime = null;
    }

    public void SwitchTurn(Player nextPlayer)
    {
        if (isGameOver) return;

        if (IsRunning)
        {
            EndTurn();
        }
        StartTurn(nextPlayer);
    }

    public bool IsOutOfTime(Player player)
    {
        if (isGameOver) return false;

        TimeSpan time = GetEffectiveTime(player);
        return time.TotalSeconds <= 0;
    }

    public string GetTimeString(Player player)
    {
        TimeSpan time = GetEffectiveTime(player);
        if (time.TotalSeconds < 0) time = TimeSpan.Zero;
        return $"{(int)time.TotalMinutes}:{time.Seconds:D2}";
    }

    public void StopClock()
    {
        if (IsRunning)
        {
            EndTurn();
        }
        isGameOver = true;
    }

    private TimeSpan GetEffectiveTime(Player player)
    {
        TimeSpan baseTime = player == Player.White ? WhiteTime : BlackTime;

        if (IsRunning && currentPlayer == player && turnStartTime.HasValue)
        {
            TimeSpan elapsed = DateTime.Now - turnStartTime.Value;
            baseTime -= elapsed;
        }

        return baseTime;
    }
}

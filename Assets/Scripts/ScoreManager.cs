using System;

public static class ScoreManager
{
    public static Action<int> onScoreChange; 
    private static int _score; 
    public static void AddScore(int value)
    {
        _score += value;
        onScoreChange?.Invoke(_score); 
    }
    public static void RemoveScore(int value)
    {
        _score -= value;

        if (_score < 0)
            _score = 0;

        onScoreChange?.Invoke(_score);
    }
}

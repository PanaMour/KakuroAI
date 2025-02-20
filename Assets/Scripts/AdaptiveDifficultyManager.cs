using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class AdaptiveDifficultyManager
{
    public DifficultyLevel CurrentDifficulty { get; private set; } = DifficultyLevel.Medium;

    public void AdjustDifficulty(AITracker tracker)
    {
        if (tracker.mistakes > 5 || tracker.elapsedTime > 300f)
        {
            CurrentDifficulty = DifficultyLevel.Easy;
        }
        else if (tracker.mistakes < 2 && tracker.elapsedTime < 120f)
        {
            CurrentDifficulty = DifficultyLevel.Hard;
        }
        else
        {
            CurrentDifficulty = DifficultyLevel.Medium;
        }
    }

    public int GetAdjustedGridSize(int baseSize)
    {
        switch (CurrentDifficulty)
        {
            case DifficultyLevel.Hard:
                return Mathf.Min(12, baseSize + 1);
            case DifficultyLevel.Easy:
                return Mathf.Max(5, baseSize - 1);
            default:
                return baseSize;
        }
    }

    public float GetAdjustedBlockedProbability(float baseProbability)
    {
        switch (CurrentDifficulty)
        {
            case DifficultyLevel.Hard:
                return Mathf.Max(0.1f, baseProbability - 0.05f);
            case DifficultyLevel.Easy:
                return Mathf.Min(0.4f, baseProbability + 0.05f);
            default:
                return baseProbability;
        }
    }
}

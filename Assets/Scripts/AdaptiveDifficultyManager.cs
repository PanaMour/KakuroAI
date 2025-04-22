using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class AdaptiveDifficultyManager
{
    public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.Medium;

    // Updates difficulty based on the performance of the previous puzzle.
    public DifficultyLevel UpdateDifficulty(DifficultyLevel currentDifficulty, float solveTime, int mistakes, int hints)
    {
        float performance = solveTime + mistakes * 0.1f + hints * 0.2f;

        string rating;
        if (performance < 10f)
            rating = "fast";
        else if (performance < 20f)
            rating = "medium";
        else
            rating = "slow";

        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                return (rating == "fast") ? DifficultyLevel.Medium : DifficultyLevel.Easy;
            case DifficultyLevel.Medium:
                if (rating == "fast")
                    return DifficultyLevel.Hard;
                else if (rating == "slow")
                    return DifficultyLevel.Easy;
                else
                    return DifficultyLevel.Medium;
            case DifficultyLevel.Hard:
                return (rating == "slow") ? DifficultyLevel.Medium : DifficultyLevel.Hard;
            default:
                return currentDifficulty;
        }
    }

    // Map difficulty to grid size.
    public int GetAdjustedGridSize(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy: return 4;
            case DifficultyLevel.Medium: return 5;
            case DifficultyLevel.Hard: return 6;
            default: return 5;
        }
    }

    // Map difficulty to blocked-cell probability.
    public float GetAdjustedBlockedProbability(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy: return 0.4f;  // Simpler puzzles: more blocked cells.
            case DifficultyLevel.Medium: return 0.2f;
            case DifficultyLevel.Hard: return 0.1f;  // Harder puzzles: fewer blocked cells.
            default: return 0.2f;
        }
    }
}

using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class AdaptiveDifficultyManager
{
    // The current difficulty level; this can be updated by your agent.
    public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.Medium;

    public void AdjustDifficulty(AITracker tracker)
    {
        //Debug.Log($"Mistakes = {tracker.mistakes}, Elapsed Time = {tracker.elapsedTime}");
        if (tracker.mistakes > 30 || tracker.elapsedTime > 15f)
        {
            CurrentDifficulty = DifficultyLevel.Easy;
        }
        else if (tracker.mistakes < 15 && tracker.elapsedTime < 10f)
        {
            CurrentDifficulty = DifficultyLevel.Hard;
        }
        else
        {
            CurrentDifficulty = DifficultyLevel.Medium;
        }
    }

    // Adjust grid size based on the current difficulty.
    // If performance is poor (Easy), reduce grid size by 1 (minimum 4).
    // If performance is good (Hard), increase grid size by 1 (maximum 7).
    // For Medium, keep grid size unchanged.
    public int GetAdjustedGridSize(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Hard: return 7;
            case DifficultyLevel.Easy: return 4;
            default: return 5; // Medium
        }
    }

    // Blocked probability tied directly to difficulty.
    public float GetAdjustedBlockedProbability(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Hard: return 0.3f; // More complex
            case DifficultyLevel.Easy: return 0.1f; // Simpler
            default: return 0.2f; // Medium
        }
    }

}

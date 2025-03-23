using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using System.Collections;

public class DifficultyAgent : Agent
{
    // Reference to your GridManager in the scene.
    public GridManager gridManager;

    private KakuroSolver solver;
    private System.Random random;
    public bool useSolver = false;  // Set to false for live play (no automatic solving)

    public override void Initialize()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        solver = new KakuroSolver();
        random = new System.Random();
    }

    public override void OnEpisodeBegin()
    {
        // For both training and live play, generate a new puzzle.
        gridManager.NewPuzzle();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalize observations based on assumed maximums.
        sensor.AddObservation(gridManager.Tracker.mistakes / 10f);    // Mistakes
        sensor.AddObservation(gridManager.Tracker.elapsedTime / 300f); // Elapsed time
        sensor.AddObservation(gridManager.Tracker.hintsUsed / 10f);    // Hints used
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!useSolver)
        {
            return;
        }

        int action = actions.DiscreteActions[0];
        var chosenDifficulty = (DifficultyLevel)action; // Store the chosen difficulty
        gridManager.adaptiveManager.CurrentDifficulty = chosenDifficulty;

        gridManager.NewPuzzle();

        StartCoroutine(ProcessEpisode(chosenDifficulty)); // Pass the difficulty here
    }

    private IEnumerator ProcessEpisode(DifficultyLevel chosenDifficulty)
    {
        yield return new WaitForSeconds(0.1f);

        var (solveTime, mistakes, hints) = solver.Solve(gridManager.kakuroPuzzle, random, chosenDifficulty);
        gridManager.Tracker.UpdateStats(solveTime, mistakes, hints);

        // Pass all 4 parameters including chosenDifficulty
        float reward = ComputeReward(solveTime, mistakes, hints, chosenDifficulty);
        AddReward(reward);

        Debug.Log($"Episode Ended - Difficulty: {chosenDifficulty}, SolveTime: {solveTime:F2}s, Mistakes: {mistakes}, Hints: {hints}, Reward: {reward}");
        EndEpisode();
    }

    // This method is called by a UI button when the player finishes a puzzle.
    public void OnPlayerFinishedPuzzle(float playerSolveTime, int playerMistakes, int playerHints)
    {
        gridManager.Tracker.UpdateStats(playerSolveTime, playerMistakes, playerHints);

        // Get current difficulty and pass all 4 parameters
        var currentDifficulty = gridManager.adaptiveManager.CurrentDifficulty;
        float reward = ComputeReward(playerSolveTime, playerMistakes, playerHints, currentDifficulty);
        AddReward(reward);

        Debug.Log($"Player Finished Puzzle - Difficulty: {currentDifficulty}, " +
                  $"SolveTime: {playerSolveTime:F2}s, Mistakes: {playerMistakes}, Hints: {playerHints}, Reward: {reward}");
        EndEpisode();
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.LeftArrow))
            discreteActions[0] = 0; // Choose Easy
        else if (Input.GetKey(KeyCode.RightArrow))
            discreteActions[0] = 2; // Choose Hard
        else
            discreteActions[0] = 1; // Default to Medium
    }

    // Updated reward function that takes difficulty into account.
    private float ComputeReward(float solveTime, int mistakes, int hints, DifficultyLevel chosenDifficulty)
    {
        float timeReward = Mathf.Clamp(1 - (solveTime / 30f), 0, 1);
        float mistakePenalty = mistakes * 0.1f;
        float hintPenalty = hints * 0.05f;
        float difficultyBonus = (int)chosenDifficulty * 0.2f;

        return timeReward + difficultyBonus - mistakePenalty - hintPenalty;
    }
}

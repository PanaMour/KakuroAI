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
        Debug.Log("==== NEW EPISODE STARTED ====");
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
        // 1. Force valid solver outputs (debug)
        float solveTime = 10f;
        int mistakes = 1;
        int hints = 0;

        try
        {
            var result = solver.Solve(gridManager.kakuroPuzzle, random, chosenDifficulty);
            solveTime = result.solveTime;
            mistakes = result.mistakes;
            hints = result.hintsUsed;
            Debug.Log($"Solver OK | Time: {solveTime}, Mistakes: {mistakes}, Hints: {hints}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Solver crashed: {e}");
        }

        // 2. Simplified reward (always non-zero)
        float reward = ComputeReward(solveTime, mistakes, hints, chosenDifficulty);

        // 3. Log and apply
        Debug.Log($"Final reward: {reward}");
        AddReward(reward);

        yield return null; // Ensure coroutine runs
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

    private float ComputeReward(float solveTime, int mistakes, int hints, DifficultyLevel chosenDifficulty)
    {
        float baseReward = 1.0f;

        float timePenalty = solveTime * 0.02f;
        float mistakePenalty = mistakes * 0.005f;
        float hintPenalty = hints * 0.005f;

        float difficultyBonus = (int)chosenDifficulty * 0.1f;

        float finalReward = baseReward + difficultyBonus - timePenalty - mistakePenalty - hintPenalty;
        return finalReward;
    }


}

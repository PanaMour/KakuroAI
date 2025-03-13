using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class DifficultyAgent : Agent
{
    // Reference to your GridManager in the scene.
    public GridManager gridManager;

    private KakuroSolver solver;
    private System.Random random;

    public override void Initialize()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        solver = new KakuroSolver();
        random = new System.Random();
    }

    public override void OnEpisodeBegin()
    {
        // Generate a new puzzle at the start of the episode.
        gridManager.NewPuzzle();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalize observations based on assumed maximums.
        sensor.AddObservation(gridManager.Tracker.mistakes / 10f);      // Mistakes
        sensor.AddObservation(gridManager.Tracker.elapsedTime / 300f);   // Elapsed time
        sensor.AddObservation(gridManager.Tracker.hintsUsed / 10f);      // Hints used
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Map discrete action: 0 = Easy, 1 = Medium, 2 = Hard.
        int action = actions.DiscreteActions[0];
        DifficultyLevel newDifficulty = DifficultyLevel.Medium;
        if (action == 0)
            newDifficulty = DifficultyLevel.Easy;
        else if (action == 2)
            newDifficulty = DifficultyLevel.Hard;

        // Apply the chosen difficulty.
        gridManager.adaptiveManager.CurrentDifficulty = newDifficulty;
        gridManager.NewPuzzle();

        StartCoroutine(ProcessEpisode());
    }

    private System.Collections.IEnumerator ProcessEpisode()
    {
        // Wait a short time so Unity can update the environment
        yield return new WaitForSeconds(0.1f);

        var (solveTime, mistakes, hints) = solver.Solve(gridManager.kakuroPuzzle, random);
        float reward = ComputeReward(solveTime, mistakes, hints);
        AddReward(reward);

        Debug.Log($"Episode Ended - SolveTime: {solveTime:F2}s, Mistakes: {mistakes}, Hints: {hints}, Reward: {reward}");
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

    // Simple reward function that penalizes longer solve times, mistakes, and hints.
    private float ComputeReward(float solveTime, int mistakes, int hints)
    {
        // Adjust these scaling factors based on your observed metrics.
        float timePenalty = solveTime / 1000f;       // If typical solve time is around 1-10s, this yields a penalty between 0.1 and 1.0.
        float mistakePenalty = mistakes * 0.005f;    // Each mistake adds a 0.5% penalty.
        float hintPenalty = hints * 0.01f;            // Each hint adds a 10% penalty.

        float finalReward = 1f - timePenalty - mistakePenalty - hintPenalty;
        return Mathf.Max(0f, finalReward);
    }


}

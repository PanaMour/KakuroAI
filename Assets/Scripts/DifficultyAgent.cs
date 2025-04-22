using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class DifficultyAgent : Agent
{
    [Header("Scene refs")]
    public GridManager gridManager;
    public bool useSolver = false;

    private KakuroSolver solver;
    private System.Random rand;

    private float lastTime = 0f;
    private int lastMist = 0;
    private int lastHints = 0;

    public override void Initialize()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        solver = new KakuroSolver();
        rand = new System.Random();
    }

    public override void OnEpisodeBegin()
    {
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Mathf.Clamp01(lastMist / 10f));
        sensor.AddObservation(Mathf.Clamp01(lastTime / 300f));
        sensor.AddObservation(Mathf.Clamp01(lastHints / 10f));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var picked = (DifficultyLevel)actions.DiscreteActions[0];
        Debug.Log($"picked {picked}");

        gridManager.adaptiveManager.CurrentDifficulty = picked;

        gridManager.NewPuzzle();

        if (useSolver)
            StartCoroutine(SolveAndFinish(picked));
    }

    IEnumerator SolveAndFinish(DifficultyLevel diff)
    {
        var result = solver.Solve(gridManager.kakuroPuzzle, rand, diff);
        float reward = ComputeReward(result.solveTime, result.mistakes, result.hintsUsed, diff);
        lastTime = result.solveTime;
        lastMist = result.mistakes;
        lastHints = result.hintsUsed;
        AddReward(reward);
        EndEpisode();
        yield return null;
    }

    public void OnPlayerFinishedPuzzle(float t, int m, int h)
    {
        lastTime = t;
        lastMist = m;
        lastHints = h;

        float reward = ComputeReward(t, m, h, gridManager.adaptiveManager.CurrentDifficulty);
        AddReward(reward);

        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.LeftArrow)) d[0] = 0;
        else if (Input.GetKey(KeyCode.RightArrow)) d[0] = 2;
        else d[0] = 1;
    }

    private float ComputeReward(float time, int mistakes, int hints, DifficultyLevel d)
    {
        return 1f
             + ((int)d) * 0.1f
             - 0.02f * time
             - 0.005f * mistakes
             - 0.005f * hints;
    }
}

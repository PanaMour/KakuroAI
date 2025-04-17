using System;
using UnityEngine;

public class KakuroSolver
{
    private float timePerGuess;

    // Set a default time cost per guess (0.1 seconds)
    public KakuroSolver(float timePerGuess = 0.1f)
    {
        this.timePerGuess = timePerGuess;
    }

    public (float solveTime, int mistakes, int hintsUsed) Solve(Kakuro puzzle, System.Random random, DifficultyLevel difficulty)
    {
        int mistakes = 0;
        int hintsUsed = 0;
        int totalGuesses = 0;

        int rows = puzzle.Grid.GetLength(0);
        int cols = puzzle.Grid.GetLength(1);

        float baseTimePerGuess = 0.1f;

        // Increase time cost if difficulty is Hard
        if (difficulty == DifficultyLevel.Hard)
            baseTimePerGuess *= 1.5f;
        else if (difficulty == DifficultyLevel.Easy)
            baseTimePerGuess *= 0.8f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (puzzle.Grid[row, col] == Kakuro.CellType.White)
                {
                    int correctDigit = puzzle.GetSolution(row, col);
                    bool guessedCorrectly = false;
                    float hintChance = 0f; // Start at 0% chance

                    while (!guessedCorrectly)
                    {
                        // Check if the solver should use a hint.
                        if (random.NextDouble() < hintChance)
                        {
                            // Use a hint, count it, and accept the correct answer.
                            hintsUsed++;
                            guessedCorrectly = true;
                            hintChance = 0f;
                        }
                        else
                        {
                            int guess = random.Next(1, 10);
                            totalGuesses++;
                            if (guess == correctDigit)
                            {
                                guessedCorrectly = true;
                            }
                            else
                            {
                                mistakes++;
                                // Increase hint chance by 5% per wrong guess (cap at 100%)
                                hintChance = Math.Min(1.0f, hintChance + 0.05f);
                            }
                        }
                    }
                }
            }
        }
        // Simulate the total time taken based on total guesses.
        float solveTime = totalGuesses * timePerGuess;
        return (solveTime, mistakes, hintsUsed);
    }
}

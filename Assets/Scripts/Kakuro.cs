using System;
using System.Collections.Generic;
using UnityEngine;

public class Kakuro
{
    public enum CellType { Blocked, White }
    public CellType[,] Grid { get; private set; }
    public int[,] HorizontalClues { get; private set; }
    public int[,] VerticalClues { get; private set; }
    private int[,] solution;

    public Kakuro(int width, int height, System.Random random,float blockedProbability = 0.1f)
    {
        Grid = new CellType[height, width];
        HorizontalClues = new int[height, width];
        VerticalClues = new int[height, width];
        solution = new int[height, width];

        GenerateRandomLayout(random, blockedProbability);
        GenerateValidSolution(random);
        CalculateClues();

    }

    void GenerateRandomLayout(System.Random random, float blockedProbability)
    {
        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                Grid[i, j] = (random.NextDouble() < blockedProbability) ?
                    CellType.Blocked :
                    CellType.White;
            }
        }

        for (int i = 0; i < Grid.GetLength(0); i++) Grid[i, 0] = CellType.Blocked;
        for (int j = 0; j < Grid.GetLength(1); j++) Grid[0, j] = CellType.Blocked;
    }

    void GenerateValidSolution(System.Random random)
    {
        // Backtracking algorithm to fill white cells
        List<(int, int)> cellsToFill = new List<(int, int)>();
        for (int i = 0; i < Grid.GetLength(0); i++)
            for (int j = 0; j < Grid.GetLength(1); j++)
                if (Grid[i, j] == CellType.White)
                    cellsToFill.Add((i, j));

        FillCells(cellsToFill, 0, random);
    }

    bool FillCells(List<(int, int)> cells, int index, System.Random random)
    {
        if (index >= cells.Count) return true;

        var (row, col) = cells[index];
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers, random);

        foreach (int num in numbers)
        {
            if (IsValidPlacement(row, col, num))
            {
                solution[row, col] = num;
                if (FillCells(cells, index + 1, random))
                    return true;
                solution[row, col] = 0;
            }
        }
        return false;
    }

    bool IsValidPlacement(int row, int col, int num)
    {
        // Check horizontal run
        List<int> horizontalNumbers = new List<int>();
        int c = col;
        while (c >= 0 && Grid[row, c] == CellType.White)
        {
            if (c != col) horizontalNumbers.Add(solution[row, c]);
            c--;
        }
        c = col + 1;
        while (c < Grid.GetLength(1) && Grid[row, c] == CellType.White)
        {
            horizontalNumbers.Add(solution[row, c]);
            c++;
        }
        if (horizontalNumbers.Contains(num)) return false;

        // Check vertical run
        List<int> verticalNumbers = new List<int>();
        int r = row;
        while (r >= 0 && Grid[r, col] == CellType.White)
        {
            if (r != row) verticalNumbers.Add(solution[r, col]);
            r--;
        }
        r = row + 1;
        while (r < Grid.GetLength(0) && Grid[r, col] == CellType.White)
        {
            verticalNumbers.Add(solution[r, col]);
            r++;
        }
        return !verticalNumbers.Contains(num);
    }


    void CalculateClues()
    {
        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                if (Grid[i, j] == CellType.Blocked)
                {
                    // Horizontal clue calculation
                    int hSum = 0;
                    int c = j + 1;
                    while (c < Grid.GetLength(1) && Grid[i, c] == CellType.White)
                    {
                        hSum += solution[i, c];
                        c++;
                    }
                    HorizontalClues[i, j] = hSum;

                    // Vertical clue calculation
                    int vSum = 0;
                    int r = i + 1;
                    while (r < Grid.GetLength(0) && Grid[r, j] == CellType.White)
                    {
                        vSum += solution[r, j];
                        r++;
                    }
                    VerticalClues[i, j] = vSum;
                }
            }
        }
    }

    void Shuffle<T>(List<T> list, System.Random random)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public int GetSolution(int row, int col)
    {
        if (row >= 0 && row < solution.GetLength(0) &&
            col >= 0 && col < solution.GetLength(1))
        {
            return solution[row, col];
        }
        return 0;
    }
}
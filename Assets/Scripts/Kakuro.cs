using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Kakuro
{
    public int[] horz;  // Sums of each column
    public int[] vert;  // Sums of each row

    // forbidden[v,h,n] = true means digit (n+1) is not allowed in cell (v,h).
    private bool[,,] forbidden;

    // Return the single fixed value for (v,h) if exactly one possibility, else 0.
    public int GetValue(int v, int h)
    {
        int value = 0;
        for (int n = 1; n <= 9; n++)
        {
            if (!forbidden[v, h, n - 1])
            {
                // If we already found a possible value, that means multiple possibilities => 0
                if (value > 0)
                    return 0;
                value = n;
            }
        }
        return value;
    }

    /// <summary>
    /// Constructor that builds a random "solution" with the given width x height,
    /// ensuring each row/column has distinct digits.  Then sets sums in horz/vert.
    /// </summary>
    public Kakuro(int width, int height, System.Random random)
    {
        // 1) Generate a full solution with backtracking
        int[,] solution = new int[height, width];
        bool success = FillGrid(solution, 0, 0, random);
        if (!success)
            throw new Exception("Could not generate a valid solution with unique row/column digits.");

        // 2) Compute sums for each row and column
        horz = new int[width];  // sums of columns
        vert = new int[height]; // sums of rows
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                horz[c] += solution[r, c];
                vert[r] += solution[r, c];
            }
        }

        // 3) Build the forbidden array so GetValue(i,j) returns the exact solution digit
        forbidden = new bool[height, width, 9];
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                int val = solution[r, c];
                // For each cell, forbid everything EXCEPT the correct digit
                for (int n = 1; n <= 9; n++)
                {
                    forbidden[r, c, n - 1] = (n != val);
                }
            }
        }
    }

    /// <summary>
    /// Backtracking: fill the grid with digits [1..9], ensuring row/col uniqueness.
    /// </summary>
    private bool FillGrid(int[,] grid, int row, int col, System.Random random)
    {
        int height = grid.GetLength(0);
        int width = grid.GetLength(1);

        // If we've filled all rows, we're done
        if (row == height)
            return true;

        // Compute next cell indices
        int nextRow = (col == width - 1) ? row + 1 : row;
        int nextCol = (col + 1) % width;

        // Shuffle digits 1..9
        List<int> digits = Enumerable.Range(1, 9)
                                     .OrderBy(_ => random.Next())
                                     .ToList();

        foreach (int d in digits)
        {
            if (CanPlace(grid, row, col, d))
            {
                grid[row, col] = d;

                if (FillGrid(grid, nextRow, nextCol, random))
                    return true;

                // backtrack
                grid[row, col] = 0;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if digit d can be placed in (row,col) without repeating in that row or col.
    /// </summary>
    private bool CanPlace(int[,] grid, int row, int col, int d)
    {
        // Check row
        for (int c = 0; c < grid.GetLength(1); c++)
        {
            if (grid[row, c] == d) return false;
        }
        // Check column
        for (int r = 0; r < grid.GetLength(0); r++)
        {
            if (grid[r, col] == d) return false;
        }
        return true;
    }
}

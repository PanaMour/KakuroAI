using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject gridCellPrefab; // Your existing prefab

    [Header("UI References")]
    public RectTransform gridPanel;
    public GridLayoutGroup gridLayout;

    [Header("Game Settings")]
    [Range(5, 12)] public int gridSize = 5;
    [Range(0.1f, 0.4f)] public float blockedCellProbability = 0.2f;

    private Kakuro kakuroPuzzle;
    private List<List<TMP_InputField>> inputFields = new List<List<TMP_InputField>>();
    private int cellSize = 80;

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        kakuroPuzzle = new Kakuro(gridSize, gridSize, new System.Random());
        SetupGridLayout();
        CreateGridUI();
    }

    void SetupGridLayout()
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize;

        // Calculate available space for the grid
        float panelWidth = gridPanel.rect.width;
        float panelHeight = gridPanel.rect.height;

        // Calculate cell size based on available space and grid size
        float cellWidth = (panelWidth - (gridLayout.spacing.x * (gridSize - 1))) / gridSize;
        float cellHeight = (panelHeight - (gridLayout.spacing.y * (gridSize - 1))) / gridSize;

        // Use the smaller dimension to ensure cells are square and fit within the panel
        cellSize = Mathf.Min((int)cellWidth, (int)cellHeight);

        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }

    void AdjustFontSize(TMP_InputField inputField)
    {
        TextMeshProUGUI textComponent = (TextMeshProUGUI)inputField.textComponent;
        if (textComponent != null)
        {
            textComponent.fontSize = Mathf.Max(12, cellSize / 2);
        }
    }

    void CreateGridUI()
    {
        ClearGrid();
        inputFields.Clear();

        for (int row = 0; row < gridSize; row++)
        {
            inputFields.Add(new List<TMP_InputField>());

            for (int col = 0; col < gridSize; col++)
            {
                GameObject cell = Instantiate(gridCellPrefab, gridPanel);
                SetupCell(cell, row, col);
                inputFields[row].Add(cell.GetComponentInChildren<TMP_InputField>());
            }
        }
    }

    void SetupCell(GameObject cell, int row, int col)
    {
        Image cellImage = cell.GetComponent<Image>();
        TMP_InputField inputField = cell.GetComponentInChildren<TMP_InputField>();

        AdjustFontSize(inputField);

        if (kakuroPuzzle.Grid[row, col] == Kakuro.CellType.Blocked)
        {
            SetupBlockedCell(cell, row, col, cellImage, inputField);
        }
        else
        {
            SetupInputCell(cell, row, col, cellImage, inputField);
        }
    }

    void SetupBlockedCell(GameObject cell, int row, int col, Image cellImage, TMP_InputField inputField)
    {
        cellImage.color = Color.black;
        inputField.gameObject.SetActive(false);

        TextMeshProUGUI verticalClue = cell.transform.Find("VerticalClue").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI horizontalClue = cell.transform.Find("HorizontalClue").GetComponent<TextMeshProUGUI>();

        verticalClue.text = kakuroPuzzle.VerticalClues[row, col] > 0 ?
            kakuroPuzzle.VerticalClues[row, col].ToString() : "";
        verticalClue.gameObject.SetActive(true);

        horizontalClue.text = kakuroPuzzle.HorizontalClues[row, col] > 0 ?
            kakuroPuzzle.HorizontalClues[row, col].ToString() : "";
        horizontalClue.gameObject.SetActive(true);

        Image diagonalLine = cell.transform.Find("DiagonalLine").GetComponent<Image>();

        RectTransform lineRect = diagonalLine.GetComponent<RectTransform>();
        lineRect.sizeDelta = new Vector2(2, Mathf.Sqrt(2) * cellSize * 0.9f);

        diagonalLine.transform.SetAsFirstSibling();
    }

    void SetupInputCell(GameObject cell, int row, int col, Image cellImage, TMP_InputField inputField)
    {
        cellImage.color = Color.white;
        inputField.gameObject.SetActive(true);

        cell.transform.Find("VerticalClue").gameObject.SetActive(false);
        cell.transform.Find("HorizontalClue").gameObject.SetActive(false);

        inputField.text = "";

    }

    void HandleInput(string value, int row, int col, TMP_InputField field)
    {
        if (int.TryParse(value, out int num) && num >= 1 && num <= 9)
        {
            field.textComponent.color = Color.black;
            ValidateRun(row, col);
        }
        else
        {
            field.text = "";
            StartCoroutine(FlashError(field));
        }
    }

    System.Collections.IEnumerator FlashError(TMP_InputField field)
    {
        field.textComponent.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        field.textComponent.color = Color.black;
    }

    void ValidateRun(int row, int col)
    {
        int horizontalStartCol = col;
        while (horizontalStartCol >= 0 &&
               kakuroPuzzle.Grid[row, horizontalStartCol] != Kakuro.CellType.Blocked)
        {
            horizontalStartCol--;
        }

        if (horizontalStartCol >= 0 && horizontalStartCol < kakuroPuzzle.Grid.GetLength(1))
        {
            int horizontalClue = kakuroPuzzle.HorizontalClues[row, horizontalStartCol];
            int horizontalSum = 0;
            List<TMP_InputField> horizontalCells = new List<TMP_InputField>();

            // Calculate sum for horizontal run
            for (int c = horizontalStartCol + 1;
                 c < kakuroPuzzle.Grid.GetLength(1) &&
                 kakuroPuzzle.Grid[row, c] == Kakuro.CellType.White;
                 c++)
            {
                horizontalSum += GetCellValue(row, c);
                horizontalCells.Add(inputFields[row][c]);
            }

            // Only validate if there's an actual run
            if (horizontalClue > 0)
            {
                UpdateCellColors(horizontalCells, horizontalSum, horizontalClue);
            }
        }

        // Vertical run validation
        int verticalStartRow = row;
        while (verticalStartRow >= 0 &&
               kakuroPuzzle.Grid[verticalStartRow, col] != Kakuro.CellType.Blocked)
        {
            verticalStartRow--;
        }

        if (verticalStartRow >= 0 && verticalStartRow < kakuroPuzzle.Grid.GetLength(0))
        {
            int verticalClue = kakuroPuzzle.VerticalClues[verticalStartRow, col];
            int verticalSum = 0;
            List<TMP_InputField> verticalCells = new List<TMP_InputField>();

            // Calculate sum for vertical run
            for (int r = verticalStartRow + 1;
                 r < kakuroPuzzle.Grid.GetLength(0) &&
                 kakuroPuzzle.Grid[r, col] == Kakuro.CellType.White;
                 r++)
            {
                verticalSum += GetCellValue(r, col);
                verticalCells.Add(inputFields[r][col]);
            }

            // Only validate if there's an actual run
            if (verticalClue > 0)
            {
                UpdateCellColors(verticalCells, verticalSum, verticalClue);
            }
        }
    }

    int GetCellValue(int row, int col)
    {
        if (row < 0 || row >= inputFields.Count ||
            col < 0 || col >= inputFields[row].Count)
        {
            return 0;
        }

        return int.TryParse(inputFields[row][col].text, out int val) ? val : 0;
    }

    void UpdateCellColors(List<TMP_InputField> cells, int sum, int clue)
    {
        Color statusColor = sum == clue ? Color.green :
                           sum > clue ? Color.red :
                           new Color(1, 0.5f, 0);

        foreach (TMP_InputField cell in cells)
        {
            cell.image.color = statusColor;
        }
    }

    void ClearGrid()
    {
        foreach (Transform child in gridPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void NewPuzzle()
    {
        InitializeGrid();
    }
}
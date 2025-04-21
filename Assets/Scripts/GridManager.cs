using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject gridCellPrefab;

    [Header("UI References")]
    public RectTransform gridPanel;
    public GridLayoutGroup gridLayout;

    [Header("Game Settings")]
    [Range(4, 7)] public int gridSize = 5;
    [Range(0.1f, 0.4f)] public float blockedCellProbability = 0.2f;

    [Header("AI Tracker UI")]
    public TMP_Text elapsedTimeText;
    public TMP_Text mistakesText;
    public TMP_Text hintsText;
    public TMP_Text difficultyText;

    public Kakuro kakuroPuzzle { get; private set; }
    public AITracker Tracker { get { return tracker; } }
    public AdaptiveDifficultyManager adaptiveManager = new AdaptiveDifficultyManager();

    private List<List<TMP_InputField>> inputFields = new List<List<TMP_InputField>>();
    private int cellSize = 80;
    private AITracker tracker = new AITracker();
    private float puzzleStartTime;

    void Start()
    {
        InitializeGrid();
        puzzleStartTime = Time.time;
    }

    void Update()
    {
        tracker.AddElapsedTime(Time.deltaTime);
        UpdateTrackerUI();
    }

    void UpdateTrackerUI()
    {
        elapsedTimeText.text = "Time: " + Mathf.FloorToInt(tracker.elapsedTime) + "s";
        mistakesText.text = "Mistakes: " + tracker.mistakes;
        hintsText.text = "Hints: " + tracker.hintsUsed;
    }

    void UpdateDifficultyUI()
    {
        difficultyText.text = "Difficulty: " + adaptiveManager.CurrentDifficulty.ToString();
    }

    void InitializeGrid()
    {
        kakuroPuzzle = new Kakuro(gridSize, gridSize, new System.Random());
        SetupGridLayout();
        CreateGridUI();
    }

    public void OnHintButtonPressed()
    {
        tracker.RecordHint();
        List<(int row, int col)> whiteCells = new List<(int, int)>();
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (kakuroPuzzle.Grid[i, j] == Kakuro.CellType.White &&
                    string.IsNullOrEmpty(inputFields[i][j].text))
                {
                    whiteCells.Add((i, j));
                }
            }
        }
        if (whiteCells.Count > 0)
        {
            var (row, col) = whiteCells[UnityEngine.Random.Range(0, whiteCells.Count)];
            inputFields[row][col].text = kakuroPuzzle.GetSolution(row, col).ToString();
        }
    }

    void SetupGridLayout()
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize;

        float panelWidth = gridPanel.rect.width;
        float panelHeight = gridPanel.rect.height;

        float cellWidth = (panelWidth - (gridLayout.spacing.x * (gridSize - 1))) / gridSize;
        float cellHeight = (panelHeight - (gridLayout.spacing.y * (gridSize - 1))) / gridSize;

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
            SetupBlockedCell(cell, row, col, cellImage, inputField);
        else
            SetupInputCell(cell, row, col, cellImage, inputField);
    }

    void SetupBlockedCell(GameObject cell, int row, int col, Image cellImage, TMP_InputField inputField)
    {
        cellImage.color = Color.black;
        inputField.gameObject.SetActive(false);

        TextMeshProUGUI verticalClue = cell.transform.Find("VerticalClue").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI horizontalClue = cell.transform.Find("HorizontalClue").GetComponent<TextMeshProUGUI>();

        verticalClue.text = kakuroPuzzle.VerticalClues[row, col] > 0 ? kakuroPuzzle.VerticalClues[row, col].ToString() : "";
        verticalClue.gameObject.SetActive(true);
        horizontalClue.text = kakuroPuzzle.HorizontalClues[row, col] > 0 ? kakuroPuzzle.HorizontalClues[row, col].ToString() : "";
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
        inputField.onEndEdit.AddListener((value) => HandleInput(value, row, col, inputField));
    }

    void HandleInput(string value, int row, int col, TMP_InputField field)
    {
        if (string.IsNullOrEmpty(value))
            return;

        if (int.TryParse(value, out int num) && num >= 1 && num <= 9)
        {
            field.textComponent.color = Color.black;
            ValidateRun(row, col);
        }
        else
        {
            field.text = "";
            tracker.RecordMistake();
            StartCoroutine(FlashError(field));
        }
    }

    System.Collections.IEnumerator FlashError(TMP_InputField field)
    {
        field.textComponent.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        field.textComponent.color = Color.black;
    }

    Image GetCellBackground(TMP_InputField inputField)
    {
        return inputField.GetComponentInParent<Image>();
    }

    void ValidateRun(int row, int col)
    {
        int horizontalStartCol = col;
        while (horizontalStartCol >= 0 && kakuroPuzzle.Grid[row, horizontalStartCol] != Kakuro.CellType.Blocked)
            horizontalStartCol--;

        if (horizontalStartCol >= 0 && horizontalStartCol < kakuroPuzzle.Grid.GetLength(1))
        {
            int horizontalClue = kakuroPuzzle.HorizontalClues[row, horizontalStartCol];
            int horizontalSum = 0;
            List<TMP_InputField> horizontalCells = new List<TMP_InputField>();
            HashSet<int> uniqueNumbers = new HashSet<int>();
            bool hasDuplicates = false;
            for (int c = horizontalStartCol + 1; c < kakuroPuzzle.Grid.GetLength(1) && kakuroPuzzle.Grid[row, c] == Kakuro.CellType.White; c++)
            {
                int val = GetCellValue(row, c);
                horizontalSum += val;
                if (val != 0 && !uniqueNumbers.Add(val))
                    hasDuplicates = true;
                horizontalCells.Add(inputFields[row][c]);
            }
            Color statusColor = Color.white;
            if (horizontalClue > 0)
            {
                if (hasDuplicates)
                    statusColor = Color.red;
                else if (horizontalSum == horizontalClue)
                    statusColor = Color.green;
                else if (horizontalSum > horizontalClue)
                    statusColor = Color.red;
                else
                    statusColor = new Color(1, 0.5f, 0); // Orange
                UpdateCellColors(horizontalCells, statusColor);
            }
        }

        int verticalStartRow = row;
        while (verticalStartRow >= 0 && kakuroPuzzle.Grid[verticalStartRow, col] != Kakuro.CellType.Blocked)
            verticalStartRow--;

        if (verticalStartRow >= 0 && verticalStartRow < kakuroPuzzle.Grid.GetLength(0))
        {
            int verticalClue = kakuroPuzzle.VerticalClues[verticalStartRow, col];
            int verticalSum = 0;
            List<TMP_InputField> verticalCells = new List<TMP_InputField>();
            HashSet<int> verticalNumbers = new HashSet<int>();
            bool hasVerticalDuplicates = false;
            for (int r = verticalStartRow + 1; r < kakuroPuzzle.Grid.GetLength(0) && kakuroPuzzle.Grid[r, col] == Kakuro.CellType.White; r++)
            {
                int val = GetCellValue(r, col);
                verticalSum += val;
                if (val != 0 && !verticalNumbers.Add(val))
                    hasVerticalDuplicates = true;
                verticalCells.Add(inputFields[r][col]);
            }
            Color verticalColor = Color.white;
            if (verticalClue > 0)
            {
                if (hasVerticalDuplicates)
                    verticalColor = Color.red;
                else if (verticalSum == verticalClue)
                    verticalColor = Color.green;
                else if (verticalSum > verticalClue)
                    verticalColor = Color.red;
                else
                    verticalColor = new Color(1, 0.5f, 0); // Orange
                UpdateCellColors(verticalCells, verticalColor);
            }
        }
    }

    int GetCellValue(int row, int col)
    {
        if (row < 0 || row >= inputFields.Count || col < 0 || col >= inputFields[row].Count)
            return 0;
        return int.TryParse(inputFields[row][col].text, out int val) ? val : 0;
    }

    void UpdateCellColors(List<TMP_InputField> cells, Color color)
    {
        foreach (TMP_InputField cell in cells)
        {
            Image background = GetCellBackground(cell);
            if (background != null)
                background.color = color;
        }
    }

    void ClearGrid()
    {
        foreach (Transform child in gridPanel)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
                img.color = Color.white;
            Destroy(child.gameObject);
        }
    }

    public void NewPuzzle()
    {
        // If performance data exists (tracker is nonzero), update the difficulty.
        if (tracker.elapsedTime > 0.1f || tracker.mistakes > 0 || tracker.hintsUsed > 0)
        {
            // Update the current difficulty based on the previous performance.
            DifficultyLevel updatedDifficulty = adaptiveManager.UpdateDifficulty(
                adaptiveManager.CurrentDifficulty,
                tracker.elapsedTime,
                tracker.mistakes,
                tracker.hintsUsed);
            adaptiveManager.CurrentDifficulty = updatedDifficulty;
        }

        // Set grid size and blocked probability based on the (updated) current difficulty.
        gridSize = adaptiveManager.GetAdjustedGridSize(adaptiveManager.CurrentDifficulty);
        blockedCellProbability = adaptiveManager.GetAdjustedBlockedProbability(adaptiveManager.CurrentDifficulty);

        // Reset tracker for the new puzzle.
        tracker.Reset();

        // Generate the new puzzle with updated parameters.
        kakuroPuzzle = new Kakuro(gridSize, gridSize, new System.Random(), blockedCellProbability);
        SetupGridLayout();
        CreateGridUI();
        UpdateDifficultyUI();

        Debug.Log($"New Puzzle Generated: GridSize = {gridSize}, Difficulty = {adaptiveManager.CurrentDifficulty}");
    }
    public void OnBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ClearUserInputs()
    {
        if (inputFields == null || inputFields.Count == 0)
            return;
        //tracker.Reset();

        /*if (elapsedTimeText != null) elapsedTimeText.text = "Time: 0s";
        if (mistakesText != null) mistakesText.text = "Mistakes: 0";
        if (hintsText != null) hintsText.text = "Hints: 0";
        if (difficultyText != null) difficultyText.text = "Difficulty: " + adaptiveManager.CurrentDifficulty;*/

        for (int r = 0; r < inputFields.Count; r++)
        {
            var rowList = inputFields[r];
            if (rowList == null) continue;

            for (int c = 0; c < rowList.Count; c++)
            {
                var inputField = rowList[c];
                if (inputField == null) continue;

                if (!inputField.gameObject.activeSelf)
                    continue;

                inputField.text = "";
                inputField.textComponent.color = Color.black;

                var bg = inputField.GetComponentInParent<Image>();
                if (bg != null)
                    bg.color = Color.white;
            }
        }
    }
}

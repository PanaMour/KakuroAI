using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject gridCellPrefab;
    public GameObject cluePrefab;
    public RectTransform gridPanel;
    public RectTransform topCluesContainer;
    public RectTransform leftCluesContainer;
    public GridLayoutGroup gridLayoutGroup;
    public int gridSize = 4;  // e.g., 4 => an internal 3x3 puzzle

    private int[,] gridData;  // user’s inputs
    private int[] topClues;   // sums for each column
    private int[] leftClues;  // sums for each row

    private Kakuro kakuroPuzzle;

    void Start()
    {
        // 1) Generate a random puzzle (which is actually a complete solution behind the scenes)
        System.Random random = new System.Random();
        kakuroPuzzle = new Kakuro(gridSize - 1, gridSize - 1, random);

        // 2) Prepare arrays for user’s input and for the displayed sums
        gridData = new int[gridSize - 1, gridSize - 1];
        topClues = new int[gridSize - 1];
        leftClues = new int[gridSize - 1];

        // Copy sums from puzzle
        for (int i = 0; i < gridSize - 1; i++)
        {
            leftClues[i] = kakuroPuzzle.vert[i];  // row sum
            topClues[i] = kakuroPuzzle.horz[i];   // column sum

            // User's grid starts empty => 0
            for (int j = 0; j < gridSize - 1; j++)
            {
                gridData[i, j] = 0;
            }
        }

        // 3) Build the UI
        AdjustCellSize(gridSize, gridSize);
        GenerateClues(gridSize, gridSize);
        GenerateGrid(gridSize, gridSize);
    }

    void AdjustCellSize(int rows, int columns)
    {
        float panelWidth = gridPanel.rect.width;
        float panelHeight = gridPanel.rect.height;

        float cellWidth = (panelWidth - (gridLayoutGroup.spacing.x * (columns - 1))) / columns;
        float cellHeight = (panelHeight - (gridLayoutGroup.spacing.y * (rows - 1))) / rows;

        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    void GenerateClues(int rows, int columns)
    {
        // Top clues (column sums)
        for (int col = 1; col < columns; col++) // skip left-most (0th) column
        {
            GameObject topClue = Instantiate(cluePrefab, topCluesContainer);
            TextMeshProUGUI clueText = topClue.GetComponent<TextMeshProUGUI>();
            if (clueText != null)
            {
                clueText.text = topClues[col - 1].ToString();
            }
        }

        // Left clues (row sums)
        for (int row = 1; row < rows; row++) // skip top-most (0th) row
        {
            GameObject leftClue = Instantiate(cluePrefab, leftCluesContainer);
            TextMeshProUGUI clueText = leftClue.GetComponent<TextMeshProUGUI>();
            if (clueText != null)
            {
                clueText.text = leftClues[row - 1].ToString();
            }
        }
    }

    void GenerateGrid(int rows, int columns)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Create a cell in the grid layout
                GameObject newCell = Instantiate(gridCellPrefab, gridPanel);

                if (i == 0 || j == 0)
                {
                    // These are the "clue" cells (gray boxes)
                    TMP_Text clueText = newCell.transform.Find("ClueText").GetComponent<TMP_Text>();
                    Image cellBackground = newCell.GetComponent<Image>();

                    if (clueText != null)
                    {
                        if (i == 0 && j > 0)
                            clueText.text = topClues[j - 1].ToString();
                        if (j == 0 && i > 0)
                            clueText.text = leftClues[i - 1].ToString();

                        clueText.alignment = TextAlignmentOptions.TopRight;
                        clueText.gameObject.SetActive(true);
                    }

                    if (cellBackground != null)
                    {
                        cellBackground.color = Color.gray;
                    }

                    // Hide the InputField for clue cells
                    TMP_InputField inputField = newCell.GetComponentInChildren<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // These are the user-input cells (white boxes)
                    int row = i - 1;
                    int col = j - 1;

                    TMP_InputField inputField = newCell.GetComponentInChildren<TMP_InputField>();
                    if (inputField != null)
                    {
                        // When user finishes typing, parse the value
                        inputField.onEndEdit.AddListener(value =>
                        {
                            int parsedValue;
                            if (int.TryParse(value, out parsedValue) && parsedValue >= 1 && parsedValue <= 9)
                            {
                                gridData[row, col] = parsedValue;
                                ValidateGrid();
                            }
                            else
                            {
                                inputField.text = "";
                                gridData[row, col] = 0;
                            }
                        });
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check each cell the user has filled; 
    /// if it matches the puzzle's hidden solution => green, else red.
    /// </summary>
    void ValidateGrid()
    {
        for (int i = 0; i < gridSize - 1; i++)
        {
            for (int j = 0; j < gridSize - 1; j++)
            {
                int correctValue = kakuroPuzzle.GetValue(i, j);
                int userValue = gridData[i, j];

                Color colorToUse = (userValue == correctValue && userValue != 0)
                                     ? Color.green
                                     : Color.red;
                HighlightCell(i, j, colorToUse);
            }
        }
    }

    /// <summary>
    /// i, j are puzzle indices in [0..gridSize-2].
    /// We find the UI cell in the gridPanel by offset (skip the top/left clue rows/columns).
    /// </summary>
    void HighlightCell(int row, int column, Color color)
    {
        // We skip the top row => row+1
        // We skip the left column => column+1
        int cellIndex = (row + 1) * gridSize + (column + 1);

        Transform cell = gridPanel.GetChild(cellIndex);
        Image cellImage = cell.GetComponent<Image>();
        if (cellImage != null)
        {
            cellImage.color = color;
        }
    }
}

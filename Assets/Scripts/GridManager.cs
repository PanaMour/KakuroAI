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
    public int gridSize = 5;

    private int[,] gridData;
    private int[] topClues;
    private int[] leftClues;

    void Start()
    {
        gridData = new int[gridSize, gridSize];
        topClues = new int[gridSize];
        leftClues = new int[gridSize];

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
        for (int j = 0; j < columns; j++)
        {
            GameObject topClue = Instantiate(cluePrefab, topCluesContainer);
            TextMeshProUGUI clueText = topClue.GetComponent<TextMeshProUGUI>();
            if (clueText != null)
            {
                topClues[j] = Random.Range(10, 30);
                clueText.text = topClues[j].ToString();
            }
        }

        for (int i = 0; i < rows; i++)
        {
            GameObject leftClue = Instantiate(cluePrefab, leftCluesContainer);
            TextMeshProUGUI clueText = leftClue.GetComponent<TextMeshProUGUI>();
            if (clueText != null)
            {
                leftClues[i] = Random.Range(10, 30);
                clueText.text = leftClues[i].ToString();
            }
        }
    }

    void GenerateGrid(int rows, int columns)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject newCell = Instantiate(gridCellPrefab, gridPanel);

                TMP_InputField inputField = newCell.GetComponentInChildren<TMP_InputField>();
                if (inputField != null)
                {
                    int row = i;
                    int column = j;

                    inputField.onEndEdit.AddListener(value =>
                    {
                        int parsedValue;
                        if (int.TryParse(value, out parsedValue) && parsedValue >= 1 && parsedValue <= 9)
                        {
                            gridData[row, column] = parsedValue;
                            Debug.Log($"Updated gridData[{row}, {column}] = {parsedValue}");
                            ValidateGrid();
                        }
                        else
                        {
                            Debug.Log($"Invalid input at [{row}, {column}]. Resetting.");
                            inputField.text = "";
                            gridData[row, column] = 0;
                        }
                    });
                }
            }
        }
    }

    void ValidateGrid()
    {
        bool[] rowValidity = new bool[gridSize];
        bool[] columnValidity = new bool[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            int rowSum = 0;
            for (int j = 0; j < gridSize; j++)
            {
                rowSum += gridData[i, j];
            }
            rowValidity[i] = rowSum == leftClues[i];
        }

        for (int j = 0; j < gridSize; j++)
        {
            int columnSum = 0;
            for (int i = 0; i < gridSize; i++)
            {
                columnSum += gridData[i, j];
            }
            columnValidity[j] = columnSum == topClues[j];
        }

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int cellIndex = i * gridSize + j;
                Transform cell = gridPanel.GetChild(cellIndex);
                Image cellImage = cell.GetComponent<Image>();

                if (cellImage != null)
                {
                    if (gridData[i, j] == 0)
                    {
                        cellImage.color = Color.white;
                    }
                    else
                    {
                        bool isValid = rowValidity[i] && columnValidity[j];
                        cellImage.color = isValid ? Color.green : Color.red;
                    }
                }
            }
        }
    }


    void HighlightRow(int rowIndex, bool isValid)
    {
        for (int j = 0; j < gridSize; j++)
        {
            int cellIndex = rowIndex * gridSize + j;
            Debug.Log($"cellindex row = {cellIndex} , {isValid}");
            Transform cell = gridPanel.GetChild(cellIndex);
            Image cellImage = cell.GetComponent<Image>();

            if (cellImage != null)
            {
                cellImage.color = isValid ? Color.green : Color.red;
            }
        }
    }


    void HighlightColumn(int columnIndex, bool isValid)
    {
        for (int i = 0; i < gridSize; i++)
        {
            int cellIndex = i * gridSize + columnIndex;
            Debug.Log($"Index col = {cellIndex} , {isValid}");
            Transform cell = gridPanel.GetChild(cellIndex);
            Image cellImage = cell.GetComponent<Image>();

            if (cellImage != null)
            {
                cellImage.color = isValid ? Color.green : Color.red;
            }
        }
    }

}

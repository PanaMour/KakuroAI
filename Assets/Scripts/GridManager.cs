using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject gridCellPrefab;
    public RectTransform gridPanel;
    public GridLayoutGroup gridLayoutGroup;
    public int gridSize = 5;

    void Start()
    {
        AdjustCellSize(gridSize, gridSize);
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

    void GenerateGrid(int rows, int columns)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject newCell = Instantiate(gridCellPrefab, gridPanel);

                TextMeshProUGUI cellText = newCell.GetComponentInChildren<TextMeshProUGUI>();
                if (cellText != null)
                {
                    cellText.text = Random.Range(1, 10).ToString();
                }
            }
        }
    }
}

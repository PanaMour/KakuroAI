using UnityEngine;
using TMPro;

public class GridManager : MonoBehaviour
{
    public GameObject gridCellPrefab;
    public Transform gridPanel;
    public int gridSize = 5;

    void Start()
    {
        GenerateGrid(gridSize, gridSize);
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

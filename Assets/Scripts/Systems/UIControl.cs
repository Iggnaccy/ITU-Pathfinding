using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridVisualizer gridVisualizer;
    [SerializeField] private TMP_InputField widthInput, heightInput, obstacleWeight, traversableWeight, coverWeight, playerMoveRange, playerAttackRange;
    [SerializeField] private Button generateButton;
    [SerializeField] private Unit player;

    private void Start()
    {
        widthInput.onEndEdit.AddListener(OnWidthOrHeightChanged);
        heightInput.onEndEdit.AddListener(OnWidthOrHeightChanged);
        obstacleWeight.onEndEdit.AddListener(OnObstacleWeightChanged);
        traversableWeight.onEndEdit.AddListener(OnTraversableWeightChanged);
        coverWeight.onEndEdit.AddListener(OnCoverWeightChanged);
        playerMoveRange.onEndEdit.AddListener(OnPlayerMoveRangeChanged);
        playerAttackRange.onEndEdit.AddListener(OnPlayerAttackRangeChanged);

        generateButton.onClick.AddListener(OnGenerateClicked);
    }

    private void OnPlayerAttackRangeChanged(string value)
    {
        if (int.TryParse(value, out int range) && range >= 0)
        {
            player.attackRange = range;
        }
    }

    private void OnPlayerMoveRangeChanged(string value)
    {
        if (int.TryParse(value, out int range) && range >= 0)
        {
            player.moveRange = range;
        }
    }

    private void OnGenerateClicked()
    {
        gridManager.GenerateGrid();
        gridVisualizer.BuildGrid();
    }

    private void OnCoverWeightChanged(string value)
    {
        if (int.TryParse(value, out int weight) && weight >= 0)
        {
            gridManager.coverWeight = weight;
        }
    }

    private void OnTraversableWeightChanged(string value)
    {
        if (int.TryParse(value, out int weight) && weight >= 0)
        {
            gridManager.traversableWeight = weight;
        }
    }

    private void OnObstacleWeightChanged(string value)
    {
        if (int.TryParse(value, out int weight) && weight >= 0)
        {
            gridManager.obstacleWeight = weight;
        }
    }

    private void OnWidthOrHeightChanged(string value)
    {
        if (int.TryParse(widthInput.text, out int width) && width > 0 && int.TryParse(heightInput.text, out int height) && height > 0)
        {
            gridManager.SetDimensions(width, height);
        }
    }
}

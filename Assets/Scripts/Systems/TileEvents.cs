using UnityEngine;
using UnityEngine.EventSystems;

public class TileEvents : MonoBehaviour
{
    private int x, y;
    private GridVisualizer gridVisualizer;

    public int X => x;
    public int Y => y;

    public void Initialize(int x, int y, GridVisualizer gridVisualizer)
    {
        this.x = x;
        this.y = y;
        this.gridVisualizer = gridVisualizer;
    }

    private static TileEvents lastClickedTile;
    public static bool EnableClick = true;

    public void OnLeftClick()
    {
        if (!EnableClick) return;

        if (lastClickedTile == this)
        {
            gridVisualizer.BeginMovementToTile(x, y);
            lastClickedTile = null;
            return;
        }

        lastClickedTile = this;
        gridVisualizer.FindPathToTile(x, y);
    }

    public void OnRightClick()
    {
        if (!EnableClick) return;
        lastClickedTile = null;
        gridVisualizer.ChangeTileType(x, y);
    }
}

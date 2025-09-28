using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Unit player, enemy;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    
    public int traversableWeight, obstacleWeight, coverWeight;

    public Tile[,] tiles;
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;

    private bool isGridSizeDirty = false;

    [SerializeField] private int dirtyWidth, dirtyHeight;

    public void SetTile(int x, int y, TileType type)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return; // Out of bounds
        tiles[x, y].Type = type;
    }

    public void SetDimensions(int width, int height)
    {
        dirtyWidth = width;
        dirtyHeight = height;
        isGridSizeDirty = true;
    }

    public void GenerateGrid()
    {
        if (isGridSizeDirty)
        {
            gridWidth = dirtyWidth;
            gridHeight = dirtyHeight;
            isGridSizeDirty = false;
        }

        tiles = new Tile[gridWidth, gridHeight];

        float totalWeight = traversableWeight + obstacleWeight + coverWeight;
        var obstacleThreshold = traversableWeight + obstacleWeight;

        List<Tile> traversableTiles = new List<Tile>();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float rand = Random.value * totalWeight;
                TileType type = rand < traversableWeight ? TileType.Traversable :
                                rand < obstacleThreshold ? TileType.Obstacle : TileType.Cover;
                tiles[x, y] = new Tile { x = x, y = y, Type = type };
                if (type == TileType.Traversable)
                {
                    traversableTiles.Add(tiles[x, y]);
                }
            }
        }

        var playerTileId = Random.Range(0, traversableTiles.Count);
        player.x = traversableTiles[playerTileId].x;
        player.y = traversableTiles[playerTileId].y;
        traversableTiles.RemoveAt(playerTileId);
        var enemyTileId = Random.Range(0, traversableTiles.Count);
        enemy.x = traversableTiles[enemyTileId].x;
        enemy.y = traversableTiles[enemyTileId].y;
    }

    private static readonly Vector2Int[] directions =
    { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public Tile[] FindPath(Vector2Int start, Vector2Int end, params TileType[] pathables)
    {
        if (!IsInBounds(start) || !IsInBounds(end))
        {
            return null; // Start or end is out of bounds
        }
        List<PathNode> openList = new List<PathNode>();
        bool[] closedSet = new bool[gridWidth * gridHeight];
        PathNode[] allNodes = new PathNode[gridWidth * gridHeight];
        int startIndex = start.y * gridWidth + start.x;
        Debug.Log($"Start Index: {startIndex}");
        int endIndex = end.y * gridWidth + end.x;
        allNodes[startIndex] = new PathNode { x = start.x, y = start.y, g = 0, f = Heuristic(start, end), parentIndex = -1, wasInitialized = true };
        openList.Add(allNodes[startIndex]);
        while (openList.Count > 0)
        {
            openList.Sort((a, b) => a.f.CompareTo(b.f));
            PathNode currentNode = openList[0];

            if(!currentNode.wasInitialized)
            {
                Debug.LogError("Uninitialized node in open list");
            }

            if (currentNode.x == end.x && currentNode.y == end.y)
            {
                Stack<Tile> path = new Stack<Tile>();
                while (currentNode.parentIndex != -1 && currentNode.wasInitialized)
                {
                    path.Push(tiles[currentNode.x, currentNode.y]);
                    currentNode = allNodes[currentNode.parentIndex];
                }
                return path.ToArray(); // Path found
            }
            openList.RemoveAt(0);
            Debug.Log($"Evaluating node at ({currentNode.x}, {currentNode.y}) with f={currentNode.f}, g={currentNode.g}");
            closedSet[currentNode.GetIndex(gridWidth)] = true;
            Vector2Int currentPos = new Vector2Int(currentNode.x, currentNode.y);
            foreach (var move in directions)
            {
                Vector2Int neighborPos = currentPos + move;
                if (!IsInBounds(neighborPos))
                    continue; // Out of bounds
                int neighborIndex = neighborPos.y * gridWidth + neighborPos.x;
                if (closedSet[neighborIndex])
                    continue; // Already evaluated
                Tile neighborTile = tiles[neighborPos.x, neighborPos.y];
                if (System.Array.IndexOf(pathables, neighborTile.Type) < 0)
                {
                    closedSet[neighborIndex] = true; // Not traversable, so mark as evaluated
                    continue; 
                }
                int tentativeG = currentNode.g + 1;
                if (!allNodes[neighborIndex].wasInitialized)
                {
                    allNodes[neighborIndex] = new PathNode
                    {
                        x = neighborTile.x,
                        y = neighborPos.y,
                        g = tentativeG,
                        f = tentativeG + Heuristic(neighborPos, end),
                        parentIndex = currentNode.GetIndex(gridWidth),
                        wasInitialized = true
                    };
                    openList.Add(allNodes[neighborIndex]);
                }
                else if (tentativeG < allNodes[neighborIndex].g)
                {
                    allNodes[neighborIndex].g = tentativeG;
                    allNodes[neighborIndex].f = tentativeG + Heuristic(neighborPos, end);
                    allNodes[neighborIndex].parentIndex = currentNode.y * gridWidth + currentNode.x;
                }
            }
        }
        return null; // No path found

    }

    private int Heuristic(Vector2Int aPos, Vector2Int bPos)
    {
        return Mathf.Abs(aPos.x - bPos.x) + Mathf.Abs(aPos.y - bPos.y);
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }
}

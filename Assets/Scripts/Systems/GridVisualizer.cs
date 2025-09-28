using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Material traversableMaterial, obstacleMaterial, coverMaterial, highlightInRangeMaterial, highlightOutOfRangeMaterial;
    [SerializeField] private GameObjectPool tilePool;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private UnitMovementAnimator unitMovementAnimator;
    [SerializeField] private Unit player, enemy;
    [Header("Settings")]
    [SerializeField] private float tileSize = 1f;

    private List<TileEvents> visualTiles = new List<TileEvents>();

    private void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager reference is missing.");
            return;
        }
        if (tilePool == null)
        {
            Debug.LogError("TilePool reference is missing.");
            return;
        }
        if (tilePrefab == null)
        {
            Debug.LogError("TilePrefab reference is missing.");
            return;
        }
        tilePool.Initialize(tilePrefab, 10);
    }

    public void BuildGrid()
    {
        foreach (var tile in visualTiles)
        {
            if (tile != null)
                tilePool.ReturnToPool(tile.gameObject);
        }
        visualTiles.Clear();
        for (int y = 0; y < gridManager.GridHeight; y++)
        {
            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                Tile tile = gridManager.tiles[x, y];
                int i = y * gridManager.GridWidth + x;
                GameObject tileObj = tilePool.GetPooledObject();
                var tileEvents = tileObj.GetComponent<TileEvents>();
                tileEvents.Initialize(x, y, this);
                tileObj.transform.parent = this.transform;
                tileObj.transform.position = new Vector3(x * tileSize, 0, y * tileSize);
                tileObj.transform.localScale = Vector3.one * tileSize;
                Renderer renderer = tileObj.GetComponent<Renderer>();
                if (renderer == null)
                {
                    Debug.LogError("Tile prefab is missing a Renderer component.");
                    tilePool.ReturnToPool(tileObj);
                    continue;
                }
                switch (tile.Type)
                {
                    case TileType.Traversable:
                        renderer.material = traversableMaterial;
                        break;
                    case TileType.Obstacle:
                        renderer.material = obstacleMaterial;
                        break;
                    case TileType.Cover:
                        renderer.material = coverMaterial;
                        break;
                }
                visualTiles.Add(tileEvents);
            }
        }

        // Deactivate any extra tiles in the pool
        for (int i = gridManager.GridWidth * gridManager.GridHeight; i < visualTiles.Count; i++)
        {
            if (visualTiles[i] != null)
            {
                tilePool.ReturnToPool(visualTiles[i].gameObject);
                visualTiles[i] = null;
            }
        }

        player.transform.position = new Vector3(player.x * tileSize, 0, player.y * tileSize);
        enemy.transform.position = new Vector3(enemy.x * tileSize, 0, enemy.y * tileSize);
        Debug.Log($"Player spawned at ({player.x}, {player.y}), Enemy spawned at ({enemy.x}, {enemy.y})");

        unitMovementAnimator.Initialize(player, enemy, tileSize);
    }

    public void ChangeTileType(int x, int y)
    {
        if (x < 0 || x >= gridManager.GridWidth || y < 0 || y >= gridManager.GridHeight)
            return; // Out of bounds
        Tile tile = gridManager.tiles[x, y];
        TileType newType = tile.Type switch
        {
            TileType.Traversable => TileType.Obstacle,
            TileType.Obstacle => TileType.Cover,
            TileType.Cover => TileType.Traversable,
            _ => tile.Type
        };
        gridManager.SetTile(x, y, newType);
        var renderer = visualTiles[tile.GetIndex(gridManager.GridWidth)].GetComponent<Renderer>();
        switch (newType)
        {
            case TileType.Traversable:
                renderer.material = traversableMaterial;
                break;
            case TileType.Obstacle:
                renderer.material = obstacleMaterial;
                break;
            case TileType.Cover:
                renderer.material = coverMaterial;
                break;
        }
        VisualizePath(false, false);
        latestPath = null;
    }

    private Tile[] latestPath;

    public bool FindPathToTile(int x, int y)
    {
        VisualizePath(false, false);
        if (x == enemy.x && y == enemy.y)
        {
            latestPath = gridManager.FindPath(new Vector2Int(player.x, player.y), new Vector2Int(x, y), TileType.Traversable, TileType.Cover);
            VisualizePath(true, true);
        }
        else
        {
            latestPath = gridManager.FindPath(new Vector2Int(player.x, player.y), new Vector2Int(x, y), TileType.Traversable);
            VisualizePath(true, false);
        }
        return latestPath != null;
    }

    private void VisualizePath(bool shown, bool isAttack)
    {
        if (latestPath == null)
            return;
        for (int i = 0; i < latestPath.Length; i++)
        {
            Tile tile = latestPath[i];
            int index = tile.GetIndex(gridManager.GridWidth);
            if (index < 0 || index >= visualTiles.Count)
                continue;
            var renderer = visualTiles.Find(tileEvent => tileEvent.X == tile.x && tileEvent.Y == tile.y).GetComponent<Renderer>();
            var tileType = gridManager.tiles[tile.x, tile.y].Type;
            if (renderer != null)
            {
                if (shown)
                {
                    if((isAttack && i < player.attackRange) || (!isAttack && i < player.moveRange))
                    {
                        renderer.material = highlightInRangeMaterial;
                    }
                    else
                    {
                        renderer.material = highlightOutOfRangeMaterial;
                    }
                }
                else
                {
                    switch (tileType)
                    {
                        case TileType.Traversable:
                            renderer.material = traversableMaterial;
                            break;
                        case TileType.Obstacle:
                            renderer.material = obstacleMaterial;
                            break;
                        case TileType.Cover:
                            renderer.material = coverMaterial;
                            break;
                    }
                }
            }
        }
    }

    public void BeginMovementToTile(int x, int y)
    {
        if (latestPath == null || latestPath.Length < 1)
        {
            Debug.Log("No valid path to move.");
            return;
        }
        if(enemy.x == x && enemy.y == y)
        {
            if (latestPath.Length > player.attackRange)
            {
                Debug.Log("Path is too long for attack.");
                return;
            }
            unitMovementAnimator.Attack(latestPath, 0.2f, () => {
                VisualizePath(false, false);
                RandomizeEnemyLocation();
                latestPath = null;
            });
            return;
        }

        if (latestPath.Length > player.moveRange)
        {
            Debug.Log("Path is too long for movement.");
            return;
        }
        unitMovementAnimator.AnimatePath(latestPath, 0.2f, () => {
            VisualizePath(false, false);
            latestPath = null;
        });
    }

    private void RandomizeEnemyLocation()
    {
        List<Tile> traversableTiles = new List<Tile>();
        for (int y = 0; y < gridManager.GridHeight; y++)
        {
            for (int x = 0; x < gridManager.GridWidth; x++)
            {
                if (gridManager.tiles[x, y].Type == TileType.Traversable && !(x == player.x && y == player.y))
                {
                    traversableTiles.Add(gridManager.tiles[x, y]);
                }
            }
        }
        if (traversableTiles.Count == 0)
        {
            Debug.LogWarning("No available traversable tiles to move the enemy.");
            return;
        }
        var enemyTileId = Random.Range(0, traversableTiles.Count);
        unitMovementAnimator.RespawnEnemy(traversableTiles[enemyTileId].x, traversableTiles[enemyTileId].y);
    }
}

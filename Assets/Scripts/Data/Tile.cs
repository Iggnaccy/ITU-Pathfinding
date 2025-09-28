using System;

[Serializable]
public struct Tile
{
    public int x, y;
    public TileType Type;

    public int GetIndex(int gridWidth)
    {
        return y * gridWidth + x;
    }
}

public enum TileType
{
    Traversable,
    Obstacle,
    Cover
}

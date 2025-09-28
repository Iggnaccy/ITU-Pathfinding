using System;

[Serializable]
public struct PathNode 
{
    public int x, y;
    public int g, f;
    public int parentIndex;
    public bool wasInitialized; // To make checking easier without nullables or explicit constructors

    public int GetIndex(int gridWidth)
    {
        return y * gridWidth + x;
    }
}

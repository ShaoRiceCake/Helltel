using UnityEngine;

public class Tile{
    public Transform tiles;
    public Transform origin;
    public Connector connector;

    public Tile(Transform _tile, Transform _origin)
    {
        tiles = _tile;
        origin = _origin;
    }
}
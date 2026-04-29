using UnityEngine;

public enum Sides
{
    None = -1,
    Top,
    Left,
    Right,
    Bottom,
}

public class Tile
{
    public int Id;
    public Tile[] adjacents = new Tile[4];
    public int autoTileId;
    public int autofowTileId;// = 15;

    public bool isVisited = false;

    public bool CanMove => autoTileId != (int)TileTypes.Empty;
    public int weight = 1;

    public void UpdateAutoTileId()
    {
        autoTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] != null)
            {
                autoTileId |= 1 << i;
            }
        }
    }

    public void UpdateFowAutoTileId()
    {
        autofowTileId = 0;
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null || adjacents[i].isVisited == false)
            {
                autofowTileId |= 1 << i;
            }
        }
    }

    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; i++)
        {
            if (adjacents[i] == null)
            {
                continue;
            }

            if(adjacents[i].Id == tile.Id)
            {
                adjacents[i] = null;
                UpdateAutoTileId();
                UpdateFowAutoTileId();
                break;
            }
        }
    }

    public void ClearAdjacents()
    {
        autoTileId = (int)TileTypes.Empty;
        for(int i = 0; i < adjacents.Length; i++)
        {
            if(adjacents[i] == null)
            {
                continue;
            }

            adjacents[i].RemoveAdjacents(this);
            adjacents[i] = null;
        }
    }
}

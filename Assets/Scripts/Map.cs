using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    Grass = 15,
    Tree,
    Hills,
    Mountains,
    Towns,
    Castle,
    Monster,
}

public class Map
{
    public int rows = 0;
    public int cols = 0;

    public Tile[] tiles;

    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        tiles = new Tile[rows *  cols];
        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for(int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < cols; ++c)
            {
                int index = r * cols + c;
                var adjacents = tiles[index].adjacents;

                if (r - 1 >= 0)
                {
                    adjacents[(int)Sides.Top] = tiles[index - cols];    // up
                }
                
                if (c + 1 < cols)
                {
                    adjacents[(int)Sides.Right] = tiles[index + 1];    // right
                }
                
                if (c - 1 >= 0)
                {
                    adjacents[(int)Sides.Left] = tiles[index - 1];    // left
                }
                
                if (r + 1 < rows)
                {
                    adjacents[(int)Sides.Bottom] = tiles[index + cols];    // down
                }
            }
        }

        for(int i = 0; i <tiles.Length; ++i)
        {
            tiles[i].UpdateAutoTileId();
        }
    }
}

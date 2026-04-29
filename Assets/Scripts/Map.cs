using System.Linq;
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

    public Tile[] CoastTiles => tiles.Where(t => t.autoTileId >= 0 && t.autoTileId < (int)TileTypes.Grass).ToArray();
    public Tile[] LandTiles => tiles.Where(t => t.autoTileId == (int)TileTypes.Grass).ToArray();

    public Tile startTile;
    public Tile castleTile;

    public void Init(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;

        tiles = new Tile[rows *  cols];
        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].Id = i;
        }

        for(int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < cols; ++c)
            {
                int index = r * cols + c;
                var adjacents = tiles[index].adjacents;

                if (r + 1 < rows)
                {
                    adjacents[(int)Sides.Bottom] = tiles[index + cols];    // down
                }
                
                if (c + 1 < cols)
                {
                    adjacents[(int)Sides.Right] = tiles[index + 1];    // right
                }
                
                if (c - 1 >= 0)
                {
                    adjacents[(int)Sides.Left] = tiles[index - 1];    // left
                }

                if (r - 1 >= 0)
                {
                    adjacents[(int)Sides.Top] = tiles[index - cols];    // up
                }
            }
        }

        for(int i = 0; i <tiles.Length; ++i)
        {
            tiles[i].UpdateAutoTileId();
            tiles[i].UpdateFowAutoTileId();
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for(int i = tiles.Length - 1; i > 0; --i)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);
        }
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);

        int total = Mathf.FloorToInt(tiles.Length * percent);
        for(int i = 0; i < total; ++i)
        {
            if(tileType == TileTypes.Empty)
            {
                tiles[i].ClearAdjacents();
            }

            tiles[i].autoTileId = (int)tileType;
            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].weight = -1;
                    break;
                case TileTypes.Grass:
                case TileTypes.Towns:
                case TileTypes.Castle:
                case TileTypes.Monster:
                    tiles[i].weight = 1;
                    break;
                case TileTypes.Tree:
                    tiles[i].weight = 2;
                    break;
                case TileTypes.Hills:
                    tiles[i].weight = 4;
                    break;
                case TileTypes.Mountains:
                    tiles[i].weight = 0; 
                    break;

            }
        }
    }

    public bool CreateIsland(float erodePercent, int erodeIterations, float lakePercent, float treePercent, float hillPercent, float mountainPercent, float townPercent, float monsterPercent)
    {
        for(int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        var temp = LandTiles;
        ShuffleTiles(temp);
        temp[0].autoTileId = (int)TileTypes.Castle;

        castleTile = temp[0];
        startTile = temp[1];

        return true;
    }
}

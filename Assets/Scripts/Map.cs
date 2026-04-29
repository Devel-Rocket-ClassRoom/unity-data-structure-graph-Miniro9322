using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Rendering;

public enum TileTypes
{
    Empty = -1,
    // 0~14
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

        tiles = new Tile[rows * cols];
        for (int i = 0; i < tiles.Length; ++i)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for (int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < cols; ++c)
            {
                var index = r * cols + c;
                var adjacents = tiles[index].adjacents;
                if ((r - 1) >= 0)
                {
                    adjacents[(int)Sides.Top] = tiles[index - cols];
                }
                if ((c + 1) < cols)
                {
                    adjacents[(int)Sides.Right] = tiles[index + 1];
                }
                if ((c - 1) >= 0)
                {
                    adjacents[(int)Sides.Left] = tiles[index - 1];
                }
                if ((r + 1) < rows)
                {
                    adjacents[(int)Sides.Bottom] = tiles[index + cols];
                }
            }
        }

        for (int i = 0; i < tiles.Length; ++i)
        {
            tiles[i].UpdateAutoTileId();
            tiles[i].UpdateFowTileId();
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i > 0; --i)
        {
            int rand = Random.Range(0, i + 1);
            (tiles[rand], tiles[i]) = (tiles[i], tiles[rand]);
        }
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);
        int total = Mathf.FloorToInt(tiles.Length * percent); 
        for (int i = 0; i < total; ++i)
        {
            if (tileType == TileTypes.Empty)
            {
                tiles[i].ClearAdjacents();
            }
            tiles[i].autoTileId = (int)tileType;
        }
    }

    public bool CreateIsland(
        float erodePercent,
        int erodeIterations,
        float lakePercent,
        float treePercent,
        float hillPercent,
        float mountainPercent,
        float townPercent,
        float monsterPercent)
    {
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        for (int i = 0; i < erodeIterations; ++i)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        var towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray();
        ShuffleTiles(towns);
        startTile = towns[0]; 
        castleTile = towns[1];
        castleTile.autoTileId = (int)TileTypes.Castle;

        var path = PathFindingAStar(startTile, castleTile);
        return path.Count > 0;
    }

    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % cols;
        int ay = a.id / cols;

        int bx = b.id % cols;
        int by = b.id / cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public List<Tile> PathFindingAStar(int startTile, int goalTile)
    {
        return PathFindingAStar(tiles[startTile], tiles[goalTile]);
    }

    public List<Tile> PathFindingAStar(Tile startTile, Tile goalTile)
    {
        List<Tile> path = new List<Tile>();
        path.Clear();
        foreach (Tile tile in tiles)
        {
            tile.ClearPreviousTile();
        }

        var visited = new HashSet<Tile>();
        var pq = new PriorityQueue<Tile, int>();
        var distances = new int[tiles.Length];
        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }
        
        distances[startTile.id] = 0;
        pq.Enqueue(startTile, distances[startTile.id] + Heuristic(startTile, goalTile));

        bool success = false;
        while (pq.Count > 0)
        {
            var currentTile = pq.Dequeue();
            if (visited.Contains(currentTile))
                continue;

            if (currentTile == goalTile)
            {
                success = true;
                break;
            }

            visited.Add(currentTile);

            foreach (var adjacent in currentTile.adjacents)
            {
                if (adjacent == null || !adjacent.CanMove || visited.Contains(adjacent))
                    continue;

                var newDistance = distances[currentTile.id] + adjacent.Weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previousTile = currentTile;
                    pq.Enqueue(adjacent, distances[adjacent.id] + Heuristic(adjacent, goalTile));
                }
            }
        }
        
        if (success)
        {
            Tile step = goalTile;
            while (step != null)
            {
                path.Add(step);
                step = step.previousTile;
            }
            path.Reverse();
        }
        return path;
    }
}
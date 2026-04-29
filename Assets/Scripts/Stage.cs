using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefabs;
    private GameObject[] tileObjs;

    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    public int erodeIteration = 2;
    [Range(0f, 0.9f)]
    public float lakePercent = 0.1f;

    [Range(0f, 0.9f)]
    public float treePercent = 0.1f;
    [Range(0f, 0.9f)]
    public float hillPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float moutainPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float townPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float monsterPercent = 0.1f;

    public Vector2 tileSize = new Vector2(16, 16);

    public Sprite[] islandSprites;
    public Sprite[] fowSprites;

    private Map map;

    public Map Map => map;

    private Camera mainCamera;

    public PlayerMovemnet playerPrefab;
    private PlayerMovemnet player;    


    private Vector3 FirstTilePos
    {
        get
        {
            var pos = transform.position;
            pos.x -= mapWidth * tileSize.x * 0.5f;
            pos.y += mapHeight * tileSize.y * 0.5f;
            pos.x += tileSize.x * 0.5f;
            pos.y -= tileSize.y * 0.5f;
            return pos;
        }
    }

    private int prevTileId = -1;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetStage();
        }
    }

    private void ResetStage()
    {
        map = new Map();
        map.Init(mapHeight, mapWidth);
        bool success = false;
        do
        {
            success = map.CreateIsland(erodePercent, erodeIteration, lakePercent,
                treePercent, hillPercent, moutainPercent, townPercent, monsterPercent);
        }
        while(!success);
        CreateGrid();
        DrawPath(map.PathFindingAStar(map.startTile, map.castleTile));
        CreatePlayer();
    }

    public void DrawPath(List<Tile> path)
    {
        foreach (var tile in tileObjs)
        {
            tile.GetComponent<SpriteRenderer>().color = Color.white;
        }

        for (int i = 0; i < path.Count; ++i)
        {
            float t = (float)i / (path.Count - 1);
            tileObjs[path[i].id].GetComponent<SpriteRenderer>().color = 
                Color.Lerp(Color.green, Color.red, t);
        }
    }

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        player = Instantiate(playerPrefab);
        player.Warp(map.startTile.id);
    }

    private void CreateGrid()
    {
        if (tileObjs != null)
        {
            foreach (var tile in tileObjs)
            {
                Destroy(tile.gameObject);
            }
        }

        tileObjs = new GameObject[mapWidth * mapHeight];

        var position = FirstTilePos;

        for (int i = 0; i < mapHeight; ++i)
        {
            for (int j = 0; j < mapWidth; ++j)
            {
                var tileId = i * mapWidth + j;
                var newGo = Instantiate(tilePrefabs, transform);
                newGo.transform.position = position;
                newGo.name = $"({i:D2}, {j:D2})";
                position.x += tileSize.x;

                tileObjs[tileId] = newGo;
                DecorateTile(tileId);
            }
            position.x = FirstTilePos.x;
            position.y -= tileSize.y;
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var tileGo = tileObjs[tileId];
        var ren = tileGo.GetComponent<SpriteRenderer>();
        if (tile.isVisited)
        {
            if (tile.autoTileId != (int)TileTypes.Empty)
            {
                ren.sprite = islandSprites[tile.autoTileId];
            }
            else
            {
                ren.sprite = null;
            }
        }
        else
        {
            ren.sprite = fowSprites[tile.fowTileId];
        }
    }

    public int visitRadius = 1;

    public void OnTileVisited(int tileId)
    {
        OnTileVisited(map.tiles[tileId]);
    }

    public void OnTileVisited(Tile tile)
    {
        int centerX = tile.id % mapWidth;
        int centerY = tile.id / mapWidth;

        for (int i = -visitRadius; i <= visitRadius; ++i)
        {
            for (int j = -visitRadius; j <= visitRadius; ++j)
            {
                int x = centerX + j;
                int y = centerY + i;
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    continue;

                int id = y * mapWidth + x;
                map.tiles[id].isVisited = true;
                DecorateTile(id);
            }
        }

        var radius = visitRadius + 1;
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
                if (i == -radius || i == radius || j == -radius || j == radius)
                {
                    int x = centerX + j;
                    int y = centerY + i;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        continue;

                    int id = y * mapWidth + x;
                    map.tiles[id].UpdateFowTileId();
                    DecorateTile(id);
                }
            }
        }
    }


    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
        return WorldPosToTileId(mainCamera.ScreenToWorldPoint(screenPos));
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        var first = FirstTilePos;
        int x = Mathf.FloorToInt((worldPos.x - first.x) / tileSize.x + 0.5f);
        int y = Mathf.FloorToInt((first.y - worldPos.y) / tileSize.y + 0.5f);
        x = Mathf.Clamp(x, 0, mapWidth - 1);
        y = Mathf.Clamp(y, 0, mapHeight - 1);
        return y * mapWidth + x;
    }

    public Vector3 GetTilePos(int y, int x)
        => FirstTilePos + new Vector3(x * tileSize.x, -y * tileSize.y);

    public Vector3 GetTilePos(int tileId)
        => GetTilePos(tileId / mapWidth, tileId % mapWidth);
}

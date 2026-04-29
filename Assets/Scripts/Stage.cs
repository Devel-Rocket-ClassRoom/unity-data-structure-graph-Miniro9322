using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Stage : MonoBehaviour
{
    public GameObject tilePrefabs;
    private GameObject[] tileObjs;
    public PlayerMovement playerPrefab;
    private PlayerMovement player;

    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    public int erodeIterations = 2;
    [Range(0f, 0.9f)]
    public float lakePercent = 0.5f;
    [Range(0f, 0.9f)]
    public float treePercent = 0.5f;
    [Range(0f, 0.9f)]
    public float hillPercent = 0.5f;
    [Range(0f, 0.9f)]
    public float mountainPercent = 0.5f;
    [Range(0f, 0.9f)]
    public float townPercent = 0.5f;
    [Range(0f, 0.9f)]
    public float monsterPercent = 0.5f;

    public Vector2 tileSize = new Vector2(16, 16);

    public Sprite[] islandSprites;
    public Sprite[] fowSprites;

    private Map map;
    private Camera cam;
    private Graph graph;

    private Vector3 FirstTilePos
    {
        get
        {
            var x = transform.position.x - mapWidth * tileSize.x / 2;
            var y = transform.position.y + mapHeight * tileSize.y / 2;
            var z = transform.position.z;

            return new Vector3(x, y, z);
        }
    }

    public Map Map => map;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            do
            {
                var tempgraph = new int[mapHeight, mapWidth];
                ResetStage();
                for (int v = 0; v < mapHeight; v++)
                {
                    for (int h = 0; h < mapWidth; h++)
                    {
                        var id = v * mapWidth + h;
                        tempgraph[v, h] = map.tiles[id].weight;
                    }
                }
                graph = new Graph();
                graph.Init(tempgraph);

                var search = new GraphSearch();
                search.Init(graph);
                if (search.AStar(graph.nodes[map.startTile.Id], graph.nodes[map.castleTile.Id]) == true)
                {
                    break;
                }
            } while (true);
        }

        if(tileObjs != null)
        {
            var screenPos = cam.ScreenToWorldPoint(Input.mousePosition);
            screenPos.z = 0f;

            var tileid = ScreenPosToTileId(screenPos);

            foreach (var tile in tileObjs)
            {
                tile.GetComponent<SpriteRenderer>().color = Color.white;
            }

            if (tileid != -1)
            {   
                tileObjs[tileid].GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }

    private void ResetStage()
    {
        map = new Map();
        map.Init(mapHeight, mapWidth);
        map.CreateIsland(erodePercent, erodeIterations, lakePercent, treePercent, hillPercent, mountainPercent, townPercent, monsterPercent);
        CreateGrid();
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        player = Instantiate(playerPrefab);
        player.Init(map.startTile.Id);
    }

    private void CreateGrid()
    {
        if (tileObjs != null)
        {
            foreach(var tile in tileObjs)
            {
                Destroy(tile.gameObject);
            }
        }

        tileObjs = new GameObject[mapWidth * mapHeight];

        var position = FirstTilePos;

        for(int i = 0; i < mapHeight; i++)
        {
            for(int j = 0; j < mapWidth; j++)
            {
                var tileId = i * mapWidth + j;

                var newGo = Instantiate(tilePrefabs, transform);
                newGo.name = $"{tileId}";
                newGo.transform.position = position;
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
        if(tile.autoTileId != (int)TileTypes.Empty)
        {
            //ren.sprite = islandSprites[tile.autoTileId];

            if (tile.isVisited == true)
            {
                ren.sprite = islandSprites[tile.autoTileId];
            }
            else
            {
                ren.sprite = fowSprites[tile.autofowTileId];
            }
        }
        else
        {
            ren.sprite = null;
        }
    }

    // 1. stage 게임 오브젝트의 포지션이 그리드의 중점이 되도록 수정
    // 2. 아래 4개 메소드 구현

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = transform.position.z;

        return WorldPosToTileId(screenPos);
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                var tileId = i * mapWidth + j;

                if (Mathf.Abs(tileObjs[tileId].transform.position.x - worldPos.x) < tileSize.x / 2 && Mathf.Abs(tileObjs[tileId].transform.position.y - worldPos.y) < tileSize.y / 2)
                return tileId;
            }
        }

        return -1;
    }

    public Vector3 GetTilePos(int y, int x)
    {
        var tileId = y * mapWidth + x;
        return GetTilePos(tileId);
    }

    public Vector3 GetTilePos(int tileId)
    {
        return tileObjs[tileId].transform.position;
    }
}

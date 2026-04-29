using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private Stage stage;
    private Animator animator;
    private int currentTileId;
    private int targetTileId;
    public int forLength = 3;
    private Graph graph;
    private Camera cam;

    private bool isMoving = false;
    private Coroutine coroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;


        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
        cam = Camera.main;
    }

    private void Update()
    {
        var direction = Sides.None;
        if (Input.GetMouseButtonDown(0))
        {
            var tempgraph = new int[stage.mapHeight, stage.mapWidth];
            for (int v = 0; v < stage.mapHeight; v++)
            {
                for (int h = 0; h < stage.mapWidth; h++)
                {
                    var id = v * stage.mapWidth + h;
                    tempgraph[v, h] = stage.Map.tiles[id].weight;
                }
            }
            graph = new();
            graph.Init(tempgraph);
            var search = new GraphSearch();
            search.Init(graph);
            if(stage.ScreenPosToTileId(cam.ScreenToWorldPoint(Input.mousePosition)) != -1)
            {
                search.AStar(graph.nodes[currentTileId], graph.nodes[stage.ScreenPosToTileId(cam.ScreenToWorldPoint(Input.mousePosition))]);
            }
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            coroutine = StartCoroutine(CoMove(search.path));
        }

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Sides.Top;
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Sides.Bottom;
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Sides.Right;
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Sides.Left;
        }

        if(direction != Sides.None)
        {
            var targetTile = stage.Map.tiles[currentTileId].adjacents[(int)direction];
            if(targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.Id);
            }
        }
    }

    public void Init(int tileId)
    {
        isMoving = false;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        currentTileId = tileId;
        transform.position = stage.GetTilePos(tileId);
        OpenFow();
    }

    public void MoveTo(int tileId)
    {
        if(isMoving == true)
        {
            return;
        }

        targetTileId = tileId;
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        coroutine = StartCoroutine(CoMove());
    }

    private void OpenFow()
    {
        for (int i = -forLength; i <= forLength; i++)
        {
            for (int j = -forLength; j <= forLength; j++)
            {
                var tileId = currentTileId + i * stage.mapWidth + j;
                if(tileId < 0 || tileId >= stage.mapWidth * stage.mapHeight)
                {
                    continue;
                }
                if((currentTileId + i * stage.mapWidth) / stage.mapWidth != tileId / stage.mapWidth)
                {
                    continue;
                }
                stage.Map.tiles[tileId].isVisited = true;
                foreach(var adjacent in stage.Map.tiles[tileId].adjacents)
                {
                    if(adjacent != null)
                    {
                        adjacent.UpdateFowAutoTileId();
                        stage.DecorateTile(adjacent.Id);

                    }
                }
            }
        }
    }

    public float moveSpeed = 100f;

    private IEnumerator CoMove()
    {
        isMoving = true;
        animator.speed = 1f;
        var startPos = transform.position;
        var endPos = stage.GetTilePos(targetTileId);
        var duration = Vector3.Distance(startPos, endPos) / moveSpeed;
        var t = 0f;

        while(t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        currentTileId = targetTileId;
        transform.position = endPos;
        OpenFow();

        isMoving = false;
        animator.speed = 0f;
        coroutine = null;
    }

    private IEnumerator CoMove(List<GraphNode> path)
    {
        foreach(var node in path)
        {
            isMoving = true;
            animator.speed = 1f;
            var startPos = transform.position;
            var endPos = stage.GetTilePos(node.id);
            var duration = Vector3.Distance(startPos, endPos) / moveSpeed;
            var t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            currentTileId = node.id;
            transform.position = endPos;
            OpenFow();

            isMoving = false;
            animator.speed = 0f;
            coroutine = null;
        }
    }
}

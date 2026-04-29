using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerMovemnet : MonoBehaviour
{
    private Stage stage;
    private Animator animator;
    private int currentTileId = -1;
    private int targetTileId = -1;

    public float moveSpeed = 10f;
    private bool isMoving = false;
    private Coroutine coMove = null;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        
        var findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var clickTileId = stage.ScreenPosToTileId(Input.mousePosition);
            var targetTile = stage.Map.tiles[clickTileId];
            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.id);
            } 
        }
    }

    public void Warp(int tileId)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        isMoving = false;
        targetTileId = -1;

        animator.speed = 0f;
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);
        stage.OnTileVisited(currentTileId);
    }

    public void MoveTo(int tileId)
    {
        targetTileId = tileId;
        if (!isMoving)
        {
            coMove = StartCoroutine(CoMove());
        }
    }

    private IEnumerator CoMove()
    {
        isMoving = true;
        animator.speed = 1f;
        int currentTargetTileId = targetTileId;
        var path = stage.Map.PathFindingAStar(currentTileId, currentTargetTileId);
        if (path.Count == 0)
        {
            isMoving = false;
            animator.speed = 0f;
            coMove = null;
            yield break;
        }
        stage.DrawPath(path);
        var pathIndex = 1;
        while (pathIndex < path.Count)
        {
            if (currentTargetTileId != targetTileId)
            {
                currentTargetTileId = targetTileId;
                path = stage.Map.PathFindingAStar(currentTileId, currentTargetTileId);
                if (path.Count == 0)
                {
                    isMoving = false;
                    animator.speed = 0f;
                    coMove = null;
                    yield break;
                }
                stage.DrawPath(path);
                pathIndex = 1;
            }
            var startPos = transform.position;
            var endPos = stage.GetTilePos(path[pathIndex].id);
            var duration =  Vector3.Distance(startPos, endPos) / moveSpeed;
            var t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            transform.position = endPos;
            currentTileId = path[pathIndex].id;
            stage.OnTileVisited(currentTileId);
            ++pathIndex;
        }

        animator.speed = 0f;
        isMoving = false;
        coMove = null;
    }
}

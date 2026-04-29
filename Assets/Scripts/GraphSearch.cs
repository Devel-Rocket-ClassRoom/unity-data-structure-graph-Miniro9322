using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GraphSearch 
{
    private Graph graph;
    public List<GraphNode> path = new List<GraphNode>();

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public void DFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var stack = new Stack<GraphNode>();

        stack.Push(node);
        visited.Add(node);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent);
                stack.Push(adjacent);
            }
        }
    }

    public void BFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(node);
        visited.Add(node);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent);
                queue.Enqueue(adjacent);
            }
        }
    }

    public void DFSRecursive(GraphNode node)
    {
        path.Clear();
        DFSRecursive(node, new HashSet<GraphNode>());
    }

    protected void DFSRecursive(GraphNode node, HashSet<GraphNode> visited)
    {
        path.Add(node);
        visited.Add(node);
        foreach (var adjacent in node.adjacents)
        {
            if (!adjacent.CanVisit || visited.Contains(adjacent))
            {
                continue;
            }
            DFSRecursive(adjacent, visited);
        }
    }

    public bool PathFindingBFS(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        bool success = false;
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent);
                adjacent.previous = currentNode;
                queue.Enqueue(adjacent);
            }
        }
        
        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    public bool Dijkstra(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();
        var distances = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }
        
        distances[startNode.id] = 0;
        pq.Enqueue(startNode, distances[startNode.id]);

        bool success = false;
        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            if (visited.Contains(currentNode))
                continue;

            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    pq.Enqueue(adjacent, distances[adjacent.id]);
                }
            }
        }
        
        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    private int Heuristic(GraphNode a, GraphNode b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();
        var distances = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }
        
        distances[startNode.id] = 0;
        pq.Enqueue(startNode, distances[startNode.id] + Heuristic(startNode, endNode));

        bool success = false;
        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            if (visited.Contains(currentNode))
                continue;

            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    pq.Enqueue(adjacent, distances[adjacent.id] + Heuristic(adjacent, endNode));
                }
            }
        }
        
        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }
}

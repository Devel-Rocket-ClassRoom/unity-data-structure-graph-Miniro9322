using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public int id;
    public int weight = 1;

    public GraphNode previous = null;

    public List<GraphNode> adjacents = new();

    public bool CanVisit => adjacents.Count > 0 && weight > 0;
}

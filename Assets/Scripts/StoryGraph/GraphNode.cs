using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public string Id { get; private set; }
    public TimeOfDayPhase Phase { get; private set; }
    public Vector2 Position { get; private set; }

    public List<GraphNode> Connections { get; private set; }

    public GraphNode(string id, TimeOfDayPhase phase, Vector2 position)
    {
        Id = id;
        Phase = phase;
        Position = position;
        Connections = new List<GraphNode>();
    }

    public bool CanConnectTo(GraphNode target)
    {
        return target.Phase >= Phase;
    }

    public void Connect(GraphNode target)
    {
        if (CanConnectTo(target))
        {
            Connections.Add(target);
        }
    }
}

public enum TimeOfDayPhase
{
    Dawn = 0,
    Morning = 1,
    Noon = 2,
    Afternoon = 3,
    Evening = 4,
    Dusk = 5,
    Night = 6
}
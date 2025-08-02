using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Graph/Node")]
public class GraphNodeData : ScriptableObject
{
    public string Id;
    public TimeOfDayPhase Phase;
    public Vector2 Position;
    public GraphNodeType Type;
    public List<GraphConnection> Connections = new List<GraphConnection>();
}

public enum GraphNodeType
{
    Main,
    Alt,
    End
}
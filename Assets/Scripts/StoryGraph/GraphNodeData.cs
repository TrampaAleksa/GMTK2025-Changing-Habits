using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Graph/Node")]
public class GraphNodeData : ScriptableObject
{
    public string Id;
    public TimeOfDayPhase Phase;
    public Vector2 Position;
    public List<GraphConnection> Connections = new List<GraphConnection>();
}
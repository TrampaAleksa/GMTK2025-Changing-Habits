using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Graph/Graph")]
public class GraphData : ScriptableObject
{
    public List<GraphNodeData> Nodes = new List<GraphNodeData>();
}
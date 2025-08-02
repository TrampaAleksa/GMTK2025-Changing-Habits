using System.Collections.Generic;
using System.Linq;

public class Graph
{
    public List<GraphNode> Nodes { get; private set; }

    public Graph()
    {
        Nodes = new List<GraphNode>();
    }

    public void AddNode(GraphNode node)
    {
        Nodes.Add(node);
    }

    public GraphNode GetNode(string id)
    {
        return Nodes.FirstOrDefault(n => n.Id == id);
    }
}

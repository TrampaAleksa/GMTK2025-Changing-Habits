using UnityEditor;
using UnityEngine;
using System.Linq;

public class GraphEditorWindow : EditorWindow
{
    private GraphData graph;
    private Vector2 pan;
    private GraphNodeData selectedNode;
    private GraphNodeData connectFrom;

    [MenuItem("Window/Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<GraphEditorWindow>("Graph Editor");
    }

    private void OnGUI()
    {
        graph = (GraphData)EditorGUILayout.ObjectField("Graph", graph, typeof(GraphData), false);

        if (graph == null)
            return;

        var rect = new Rect(0, 40, position.width, position.height - 40);
        GUI.Box(rect, "");

        DrawNodes();
        ProcessEvents(Event.current);

        if (GUI.changed)
            Repaint();
    }

    private void DrawNodes()
    {
        foreach (var node in graph.Nodes)
        {
            var nodeRect = new Rect(node.Position + pan, new Vector2(120, 60));
            GUI.Box(nodeRect, $"{node.Id}\n{node.Phase}");

            if (Event.current.type == EventType.MouseDown && nodeRect.Contains(Event.current.mousePosition))
            {
                selectedNode = node;
                if (connectFrom != null && connectFrom != selectedNode)
                {
                    // Try to connect
                    if (selectedNode.Phase >= connectFrom.Phase)
                    {
                        Undo.RecordObject(connectFrom, "Connect Nodes");
                        connectFrom.Connections.Add(new GraphConnection { Target = selectedNode });
                    }
                    connectFrom = null;
                    GUI.changed = true;
                }
                else if (Event.current.control) // Ctrl+Click to start a connection
                {
                    connectFrom = selectedNode;
                }
                else
                {
                    Selection.activeObject = node;
                }
                Event.current.Use();
            }

            // Draw connections
            foreach (var conn in node.Connections)
            {
                if (conn.Target != null)
                {
                    Handles.DrawLine(node.Position + pan + new Vector2(60, 30),
                                     conn.Target.Position + pan + new Vector2(60, 30));
                }
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        if (selectedNode != null && e.type == EventType.MouseDrag && e.button == 0 && !e.control)
        {
            Undo.RecordObject(selectedNode, "Move Node");
            selectedNode.Position += e.delta;
            GUI.changed = true;
        }

        if (e.type == EventType.MouseDown && e.button == 1) // Right-click
        {
            var mousePos = e.mousePosition - pan;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Node"), false, () => CreateNode(mousePos));
            menu.ShowAsContext();
        }
    }

    private void CreateNode(Vector2 position)
    {
        var node = ScriptableObject.CreateInstance<GraphNodeData>();
        node.Id = $"Node{graph.Nodes.Count}";
        node.Position = position;
        node.Phase = TimeOfDayPhase.Dawn;
        AssetDatabase.AddObjectToAsset(node, graph);
        AssetDatabase.SaveAssets();
        graph.Nodes.Add(node);
        EditorUtility.SetDirty(graph);
    }
}

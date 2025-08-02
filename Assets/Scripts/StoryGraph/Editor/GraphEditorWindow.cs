using UnityEditor;
using UnityEngine;
using System.Linq;

public class GraphEditorWindow : EditorWindow
{
    private GraphData graph;
    private Vector2 pan;
    private GraphNodeData selectedNode;
    private GraphNodeData connectFrom;
    private const float NodeWidth = 140f;
    private const float NodeHeight = 70f;

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

        BeginWindows();
        DrawConnections();
        DrawNodes();
        EndWindows();

        ProcessEvents(Event.current);

        if (GUI.changed)
            Repaint();
    }

    private void DrawConnections()
    {
        Handles.color = Color.white;
        foreach (var node in graph.Nodes)
        {
            foreach (var conn in node.Connections)
            {
                if (conn.Target != null)
                {
                    Handles.DrawLine(
                        node.Position + pan + new Vector2(NodeWidth / 2, NodeHeight / 2),
                        conn.Target.Position + pan + new Vector2(NodeWidth / 2, NodeHeight / 2)
                    );
                }
            }
        }
    }

    private void DrawNodes()
    {
        for (int i = 0; i < graph.Nodes.Count; i++)
        {
            var node = graph.Nodes[i];
            var rect = new Rect(node.Position + pan, new Vector2(NodeWidth, NodeHeight));
            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(rect);

            // Editable ID
            EditorGUI.BeginChangeCheck();
            string newId = EditorGUILayout.TextField(node.Id);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(node, "Rename Node");
                node.Id = newId;
                node.name = newId; // this changes the sub-asset name visible in the Project
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(graph);
            }

            // Editable Phase
            EditorGUI.BeginChangeCheck();
            var newPhase = (TimeOfDayPhase)EditorGUILayout.EnumPopup(node.Phase);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(node, "Change Phase");
                node.Phase = newPhase;
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(graph);
            }

            GUILayout.EndArea();
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

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            var clickedNode = GetNodeAtPosition(e.mousePosition);
            if (clickedNode != null)
            {
                selectedNode = clickedNode;

                // If we already chose a "from" node and now clicked on another
                if (connectFrom != null && connectFrom != clickedNode)
                {
                    // Connect
                    if (clickedNode.Phase >= connectFrom.Phase)
                    {
                        Undo.RecordObject(connectFrom, "Connect Nodes");
                        connectFrom.Connections.Add(new GraphConnection { Target = clickedNode });
                        EditorUtility.SetDirty(connectFrom);
                    }
                    connectFrom = null;
                }
                else if (e.control)
                {
                    // Start a connection
                    connectFrom = clickedNode;
                }

                Selection.activeObject = clickedNode;
                e.Use();
            }
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            var mousePos = e.mousePosition;

            // Try to find a connection close to the click
            const float lineClickThreshold = 10f;
            foreach (var node in graph.Nodes)
            {
                for (int c = node.Connections.Count - 1; c >= 0; c--)
                {
                    var conn = node.Connections[c];
                    if (conn.Target == null) continue;

                    Vector2 start = node.Position + pan + new Vector2(NodeWidth / 2, NodeHeight / 2);
                    Vector2 end = conn.Target.Position + pan + new Vector2(NodeWidth / 2, NodeHeight / 2);

                    if (IsPointNearLine(mousePos, start, end, lineClickThreshold))
                    {
                        // Remove the connection
                        Undo.RecordObject(node, "Remove Connection");
                        node.Connections.RemoveAt(c);
                        EditorUtility.SetDirty(node);
                        GUI.changed = true;
                        e.Use();
                        return; // Exit, we handled the click
                    }
                }
            }

            // If no line was hit, show context menu (create node)
            var localPos = mousePos - pan;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Node"), false, () => CreateNode(localPos));
            menu.ShowAsContext();
        }

    }
    
    private bool IsPointNearLine(Vector2 point, Vector2 a, Vector2 b, float maxDistance)
    {
        // Closest point on line segment formula
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 closest = a + t * ab;
        float distance = Vector2.Distance(point, closest);
        return distance <= maxDistance;
    }


    private GraphNodeData GetNodeAtPosition(Vector2 pos)
    {
        foreach (var node in graph.Nodes)
        {
            var rect = new Rect(node.Position + pan, new Vector2(NodeWidth, NodeHeight));
            if (rect.Contains(pos))
                return node;
        }
        return null;
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

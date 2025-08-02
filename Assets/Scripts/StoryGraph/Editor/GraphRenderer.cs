using UnityEditor;
using UnityEngine;

public class GraphRenderer
{
    private const float NodeWidth = 180f;
    private const float NodeHeight = 70f;

    public void DrawConnections(GraphEditorState state)
    {
        Handles.color = Color.white;
        foreach (var node in state.Graph.Nodes)
        {
            foreach (var conn in node.Connections)
            {
                if (conn.Target == null) continue;

                Vector2 start = node.Position + state.Pan + new Vector2(NodeWidth / 2, NodeHeight / 2);
                Vector2 end = conn.Target.Position + state.Pan + new Vector2(NodeWidth / 2, NodeHeight / 2);
                Handles.DrawLine(start, end);
            }
        }
    }

    public void DrawNodes(GraphEditorState state)
    {
        foreach (var node in state.Graph.Nodes)
        {
            float contentHeight = CalculateNodeHeight(node);
            var rect = new Rect(
                node.Position + state.Pan,
                new Vector2(NodeWidth, contentHeight)
            );

            GUI.Box(rect, GUIContent.none);

            GUILayout.BeginArea(rect);
            DrawNodeContents(node, state.Graph);
            GUILayout.EndArea();
        }
    }


    private void DrawNodeContents(GraphNodeData node, GraphData graph)
    {
        DrawNodeIdField(node, graph);
        DrawNodePhaseField(node, graph);
        DrawNodeConnections(node);
    }

    private void DrawNodeIdField(GraphNodeData node, GraphData graph)
    {
        EditorGUI.BeginChangeCheck();
        string newId = EditorGUILayout.TextField(node.Id);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(node, "Rename Node");
            node.Id = newId;
            node.name = newId;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
    }

    private void DrawNodePhaseField(GraphNodeData node, GraphData graph)
    {
        EditorGUI.BeginChangeCheck();
        var newPhase = (TimeOfDayPhase)EditorGUILayout.EnumPopup(node.Phase);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(node, "Change Phase");
            node.Phase = newPhase;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
    }

    private void DrawNodeConnections(GraphNodeData node)
    {
        for (int c = node.Connections.Count - 1; c >= 0; c--)
        {
            var conn = node.Connections[c];
            GUILayout.BeginHorizontal();
            GUILayout.Label(conn.Target != null ? conn.Target.Id : "Missing", GUILayout.Width(80));
            if (GUILayout.Button("x", GUILayout.Width(20)))
            {
                Undo.RecordObject(node, "Remove Connection");
                node.Connections.RemoveAt(c);
                EditorUtility.SetDirty(node);
            }
            GUILayout.EndHorizontal();
        }
    }


    public Rect GetNodeRect(GraphNodeData node, GraphEditorState state)
    {
        float contentHeight = CalculateNodeHeight(node);
        return new Rect(node.Position + state.Pan, new Vector2(NodeWidth, contentHeight));
    }

    public Vector2 GetNodeCenter(GraphNodeData node, GraphEditorState state)
    {
        float contentHeight = CalculateNodeHeight(node);
        return node.Position + state.Pan + new Vector2(NodeWidth / 2, contentHeight / 2);
    }

    
    private float CalculateNodeHeight(GraphNodeData node)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight + 2f;
        float height = 0f;

        // ID field
        height += lineHeight;
        // Phase field
        height += lineHeight;
        // Connections
        height += node.Connections.Count * lineHeight;
        // Padding at bottom
        height += 20f;

        return height;
    }

}
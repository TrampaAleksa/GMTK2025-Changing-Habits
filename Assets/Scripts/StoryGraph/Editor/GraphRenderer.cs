using UnityEditor;
using UnityEngine;

public class GraphRenderer
{
    private const float NodeWidth = 260f;
    private const float NodeHeight = 70f;

    public void DrawConnections(GraphEditorState state)
    {
        Handles.color = Color.white;
        foreach (var node in state.Graph.Nodes)
        {
            foreach (var conn in node.Connections)
            {
                if (conn.Target == null) continue;

                Vector2 start = GetNodeCenter(node, state);
                Vector2 end = GetNodeCenter(conn.Target, state);

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
                node.Position,
                new Vector2(NodeWidth, contentHeight)
            );

            DrawNodeBackground(rect, node);

            GUILayout.BeginArea(rect);
            DrawNodeContents(node, state.Graph);
            GUILayout.EndArea();
        }
    }
    
    private const float BackgroundPadding = 10f;

    private void DrawNodeBackground(Rect rect, GraphNodeData node)
    {
        var backgroundRect = new Rect(
            rect.x - BackgroundPadding,
            rect.y - BackgroundPadding,
            rect.width + 2 * BackgroundPadding,
            rect.height + 2 * BackgroundPadding
        );

        Color fillColor = GetNodeColor(node.Type);
        Color outlineColor = Color.black;

        Handles.DrawSolidRectangleWithOutline(backgroundRect, fillColor, outlineColor);

        // Draw Uniy's GUI.Box (kept for border & style)
        GUI.Box(rect, GUIContent.none);
    }


    private void DrawNodeContents(GraphNodeData node, GraphData graph)
    {
        DrawNodeIdField(node, graph);
        DrawNodePhaseField(node, graph);
        DrawNodeTypeField(node, graph);  
        DrawNodeScenePrefabField(node, graph);  
        DrawNodeConnections(node);
    }

    private void DrawNodeIdField(GraphNodeData node, GraphData graph)
    {
        var style = new GUIStyle(EditorStyles.textField)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };

        EditorGUI.BeginChangeCheck();
        string newId = EditorGUILayout.TextField(node.Id, style, GUILayout.Height(24));
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
    
    private void DrawNodeTypeField(GraphNodeData node, GraphData graph)
    {
        EditorGUI.BeginChangeCheck();
        var newType = (GraphNodeType)EditorGUILayout.EnumPopup(node.Type);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(node, "Change Node Type");
            node.Type = newType;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
    }

    private void DrawNodeScenePrefabField(GraphNodeData node, GraphData graph)
    {
        GUILayout.Space(4);

        using (new GUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Scene Prefab", EditorStyles.boldLabel);
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            GUIStyle labelStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Italic
            };

            string label = node.StoryScenePrefab != null ? node.StoryScenePrefab.name : "< None >";
            EditorGUILayout.LabelField(label, labelStyle, GUILayout.Height(22));

            if (GUILayout.Button("Select", GUILayout.Width(70), GUILayout.Height(22)))
            {
                StoryScenePickerWindow.Show(selected =>
                {
                    Undo.RecordObject(node, "Assign Scene Prefab");
                    node.StoryScenePrefab = selected;
                    EditorUtility.SetDirty(node);
                    EditorUtility.SetDirty(graph);
                });
            }

            EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(4);
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
        return new Rect(node.Position, new Vector2(NodeWidth, contentHeight));
    }

    public Vector2 GetNodeCenter(GraphNodeData node, GraphEditorState state)
    {
        float contentHeight = CalculateNodeHeight(node);
        return node.Position + new Vector2(NodeWidth / 2, contentHeight / 2);
    }

    
    private float CalculateNodeHeight(GraphNodeData node)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight + 2f;
        float height = 0f;

        // ID field
        height += lineHeight * 2;
        // Phase field
        height += lineHeight;
        // Type field
        height += lineHeight;
        // Story Scene field
        height += lineHeight * 3;
        // Connections
        height += node.Connections.Count * lineHeight;
        // Padding at bottom
        height += 20f;

        return height;
    }
    
    private Color GetNodeColor(GraphNodeType type)
    {
        switch (type)
        {
            case GraphNodeType.Main: return new Color(0.2f, 0.4f, 0.8f, 1f);  // Blue
            case GraphNodeType.Alt:  return new Color(0.2f, 0.8f, 0.4f, 1f);  // Green
            default:                 return new Color(0.3f, 0.3f, 0.3f, 1f);
        }
    }

}
using UnityEditor;
using UnityEngine;

public class GraphRenderer
{
    private const float NodeBaseWidth = 200f;

    private GraphViewHandler _viewHandler;

    public void DrawConnections(GraphViewHandler viewHandler, GraphEditorState state)
    {
        _viewHandler = viewHandler;
        
        foreach (var node in state.Graph.Nodes)
        {
            foreach (var conn in node.Connections)
            {
                if (conn.Target == null)
                    continue;

                Handles.color = GetConnectionColor(conn.Target);

                Vector2 start = GetNodeCenterScreen(node, state);
                Vector2 end = GetNodeCenterScreen(conn.Target, state);

                Handles.DrawLine(start, end);
            }
        }

        Handles.color = Color.white;
    }

    public void DrawNodes(GraphViewHandler viewHandler, GraphEditorState state)
    {
        _viewHandler = viewHandler;
        foreach (var node in state.Graph.Nodes)
        {
            float contentHeight = CalculateNodeHeight(node);

            Vector2 drawPos = _viewHandler.GraphToScreen(node.Position, state);
            Vector2 drawSize = new Vector2(NodeBaseWidth, contentHeight) * state.Zoom;

            var rect = new Rect(drawPos, drawSize);

            DrawNodeBackground(rect, node, state.Zoom);

            GUI.BeginGroup(rect);
            DrawNodeContentsManual(node, state.Graph, rect, state.Zoom);
            GUI.EndGroup();
        }
    }

    private void DrawNodeBackground(Rect rect, GraphNodeData node, float zoom)
    {
        Handles.DrawSolidRectangleWithOutline(rect, new Color(0.15f, 0.15f, 0.15f, 1f), Color.black);

        float tagSize = 28f * zoom;
        float margin = 4f * zoom;

        float tagX = rect.x + rect.width - tagSize - margin;
        float tagY = rect.y + margin;

        Rect tagRect = new Rect(tagX, tagY, tagSize, tagSize);

        Color tagColor = GetNodeColor(node.Phase);
        Handles.DrawSolidRectangleWithOutline(tagRect, tagColor, Color.black);
    }



    private void DrawNodeContentsManual(GraphNodeData node, GraphData graph, Rect rect, float zoom)
    {
        // scale font sizes slightly with zoom
        float scale = Mathf.Clamp(zoom, 0.75f, 2f);
        float y = 5f;
        float padding = 5f;
        float fieldHeight = 20f * scale;
        float fullWidth = rect.width - 2 * padding;

        var idStyle = new GUIStyle(EditorStyles.textField)
        {
            fontSize = Mathf.RoundToInt(20 * scale),
            fontStyle = FontStyle.Bold
        };

        // ID
        EditorGUI.BeginChangeCheck();
        string newId = EditorGUI.TextField(new Rect(padding, y, fullWidth, 24 * scale), node.Id, idStyle);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(node, "Rename Node");
            node.Id = newId;
            node.name = newId;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
        y += 28 * scale;

        // Phase
        EditorGUI.BeginChangeCheck();
        var newPhase = (TimeOfDayPhase)EditorGUI.EnumPopup(new Rect(padding, y, fullWidth, fieldHeight), node.Phase);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(node, "Change Phase");
            node.Phase = newPhase;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
        y += fieldHeight + 4;

        // Type
        EditorGUI.BeginChangeCheck();
        var newType = (GraphNodeType)EditorGUI.EnumPopup(new Rect(padding, y, fullWidth, fieldHeight), node.Type);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(node, "Change Node Type");
            node.Type = newType;
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
        y += fieldHeight + 6;

        // Scene prefab header
        EditorGUI.LabelField(new Rect(padding, y, fullWidth, fieldHeight), "Scene Prefab", EditorStyles.boldLabel);
        y += fieldHeight + 2;

        // Scene prefab label + button
        string prefabName = node.StoryScenePrefab != null ? node.StoryScenePrefab.name : "< None >";
        float buttonWidth = 70f * scale;
        EditorGUI.LabelField(new Rect(padding, y, fullWidth - buttonWidth - 5, fieldHeight), prefabName, EditorStyles.helpBox);

        if (GUI.Button(new Rect(rect.width - buttonWidth - padding, y, buttonWidth, fieldHeight), "Select"))
        {
            StoryScenePickerWindow.Show(selected =>
            {
                Undo.RecordObject(node, "Assign Scene Prefab");
                node.StoryScenePrefab = selected;
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(graph);
            });
        }
        y += fieldHeight + 8;

        // Connections
        for (int c = node.Connections.Count - 1; c >= 0; c--)
        {
            var conn = node.Connections[c];
            string label = conn.Target != null ? conn.Target.Id : "Missing";

            Rect labelRect = new Rect(padding, y, fullWidth - 25, fieldHeight);
            EditorGUI.LabelField(labelRect, label);

            Rect buttonRect = new Rect(rect.width - 25, y, 20, fieldHeight);
            if (GUI.Button(buttonRect, "x"))
            {
                Undo.RecordObject(node, "Remove Connection");
                node.Connections.RemoveAt(c);
                EditorUtility.SetDirty(node);
            }
            y += fieldHeight + 2;
        }
    }

    public Rect GetNodeRect(GraphNodeData node, GraphEditorState state)
    {
        float contentHeight = CalculateNodeHeight(node);
        Vector2 pos = _viewHandler.GraphToScreen(node.Position, state);
        Vector2 size = new Vector2(NodeBaseWidth, contentHeight) * state.Zoom;
        return new Rect(pos, size);
    }

    public Vector2 GetNodeCenterScreen(GraphNodeData node, GraphEditorState state)
    {
        float contentHeight = CalculateNodeHeight(node);
        Vector2 size = new Vector2(NodeBaseWidth, contentHeight);
        return _viewHandler.GraphToScreen(node.Position + size / 2, state);
    }

    private float CalculateNodeHeight(GraphNodeData node)
    {
        float lineHeight = 22f;
        float height = 0f;

        height += 28;
        height += lineHeight;
        height += lineHeight;
        height += lineHeight * 2;
        height += node.Connections.Count * lineHeight;
        height += 20f;

        return height;
    }

    private Color GetNodeColor(TimeOfDayPhase phase)
    {
        switch (phase)
        {
            case TimeOfDayPhase.Dawn: return new Color(0.9f, 0.7f, 0.3f, 1f);
            case TimeOfDayPhase.Morning: return new Color(0.5f, 0.8f, 1f, 1f);
            case TimeOfDayPhase.Noon: return new Color(0.9f, 0.9f, 0.5f, 1f);
            case TimeOfDayPhase.Afternoon: return new Color(0.9f, 0.6f, 0.4f, 1f);
            case TimeOfDayPhase.Evening: return new Color(0.7f, 0.5f, 0.8f, 1f);
            case TimeOfDayPhase.Night: return new Color(0.3f, 0.3f, 0.7f, 1f);
            default: return new Color(0.4f, 0.4f, 0.4f, 1f);
        }
    }

    private Color GetConnectionColor(GraphNodeData targetNode)
    {
        switch (targetNode.Type)
        {
            case GraphNodeType.Main:
                return new Color(0.2f, 1f, 0.2f, 1f);
            case GraphNodeType.Alt:
                return new Color(1f, 0.8f, 0.2f, 1f);
            default:
                return Color.white;
        }
    }
}

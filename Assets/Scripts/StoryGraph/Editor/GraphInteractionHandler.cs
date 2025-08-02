using UnityEditor;
using UnityEngine;

public class GraphInteractionHandler
{
    private const float LineClickThreshold = 10f;

    // Converts screen coordinates (mouse) into graph-space coordinates
    private Vector2 ScreenToGraph(Vector2 screenPos, GraphEditorState state, Vector2 windowSize)
    {
        // Currently pivot is top-left (0,0) because BeginView does not offset by window center
        return (screenPos - state.Pan) / state.Zoom;
    }

    public void ProcessEvents(Event e, GraphEditorState state, GraphRenderer renderer, Vector2 windowSize)
    {
        if (e.type == EventType.MouseDown && e.button == 0)
            HandleLeftClick(e, state, renderer, windowSize);

        if (e.type == EventType.MouseDrag && e.button == 0 && state.SelectedNode != null && !e.control)
            HandleNodeDrag(e, state);

        if (e.type == EventType.MouseDown && e.button == 1)
            HandleRightClick(e, state, renderer, windowSize);
    }

    private void HandleLeftClick(Event e, GraphEditorState state, GraphRenderer renderer, Vector2 windowSize)
    {
        Vector2 graphPos = ScreenToGraph(e.mousePosition, state, windowSize);
        var clickedNode = GetNodeAtPosition(graphPos, state, renderer);
        if (clickedNode == null)
            return;

        state.SelectedNode = clickedNode;

        if (state.ConnectFrom != null && state.ConnectFrom != clickedNode)
        {
            TryConnectNodes(state.ConnectFrom, clickedNode);
            state.ConnectFrom = null;
        }
        else if (e.control)
        {
            state.ConnectFrom = clickedNode;
        }

        Selection.activeObject = clickedNode;
        e.Use();
    }

    private void HandleNodeDrag(Event e, GraphEditorState state)
    {
        Undo.RecordObject(state.SelectedNode, "Move Node");
        // delta is in screen coords, so apply directly
        state.SelectedNode.Position += e.delta / state.Zoom;
        GUI.changed = true;
    }

    private void HandleRightClick(Event e, GraphEditorState state, GraphRenderer renderer, Vector2 windowSize)
    {
        // Try to remove connection first
        if (TryRemoveConnectionByClick(e.mousePosition, state, renderer, windowSize))
        {
            GUI.changed = true;
            e.Use();
            return;
        }

        Vector2 graphMousePos = ScreenToGraph(e.mousePosition, state, windowSize);
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Node"), false, () => CreateNode(graphMousePos, state));
        menu.ShowAsContext();
    }

    private void TryConnectNodes(GraphNodeData from, GraphNodeData to)
    {
        if (to.Phase >= from.Phase)
        {
            Undo.RecordObject(from, "Connect Nodes");
            from.Connections.Add(new GraphConnection { Target = to });
            EditorUtility.SetDirty(from);
        }
    }

    private bool TryRemoveConnectionByClick(Vector2 clickPos, GraphEditorState state, GraphRenderer renderer, Vector2 windowSize)
    {
        Vector2 graphPos = ScreenToGraph(clickPos, state, windowSize);

        foreach (var node in state.Graph.Nodes)
        {
            for (int c = node.Connections.Count - 1; c >= 0; c--)
            {
                var conn = node.Connections[c];
                if (conn.Target == null) continue;

                Vector2 start = renderer.GetNodeCenter(node, state);
                Vector2 end = renderer.GetNodeCenter(conn.Target, state);

                if (IsPointNearLine(graphPos, start, end, LineClickThreshold))
                {
                    Undo.RecordObject(node, "Remove Connection");
                    node.Connections.RemoveAt(c);
                    EditorUtility.SetDirty(node);
                    return true;
                }
            }
        }
        return false;
    }

    private GraphNodeData GetNodeAtPosition(Vector2 graphPos, GraphEditorState state, GraphRenderer renderer)
    {
        foreach (var node in state.Graph.Nodes)
        {
            var rect = renderer.GetNodeRect(node, state);
            if (rect.Contains(graphPos))
                return node;
        }
        return null;
    }

    private bool IsPointNearLine(Vector2 point, Vector2 a, Vector2 b, float maxDistance)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 closest = a + t * ab;
        return Vector2.Distance(point, closest) <= maxDistance;
    }

    private void CreateNode(Vector2 position, GraphEditorState state)
    {
        var node = ScriptableObject.CreateInstance<GraphNodeData>();
        node.Id = $"Node{state.Graph.Nodes.Count}";
        node.Position = position;
        node.Phase = TimeOfDayPhase.Dawn;
        node.name = node.Id;

        AssetDatabase.AddObjectToAsset(node, state.Graph);
        AssetDatabase.SaveAssets();
        state.Graph.Nodes.Add(node);
        EditorUtility.SetDirty(state.Graph);
    }
}

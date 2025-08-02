using UnityEditor;
using UnityEngine;

public class GraphInteractionHandler
{
    private const float LineClickThreshold = 10f;
    private GraphViewHandler _viewHandler;


    public void ProcessEvents(GraphViewHandler viewHandler, Event e, GraphEditorState state, GraphRenderer renderer, Vector2 windowSize)
    {
        _viewHandler = viewHandler;
        
        if (e.type == EventType.MouseDown && e.button == 0)
            HandleLeftClick(e, state, renderer);

        if (e.type == EventType.MouseDrag && e.button == 0 && state.SelectedNode != null && !e.control)
            HandleNodeDrag(e, state, windowSize);

        if (e.type == EventType.MouseDown && e.button == 1)
            HandleRightClick(e, state, renderer, windowSize);
    }

    private void HandleLeftClick(Event e, GraphEditorState state, GraphRenderer renderer)
    {
        Vector2 graphPos = _viewHandler.ScreenToGraph(e.mousePosition, state);
        var clickedNode = GetNodeAtPosition(graphPos, state);
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

    private void HandleNodeDrag(Event e, GraphEditorState state, Vector2 windowSize)
    {
        Undo.RecordObject(state.SelectedNode, "Move Node");
        state.SelectedNode.Position += e.delta / state.Zoom;

        // Optional auto-pan
        const float edge = 30f;
        const float panSpeed = 10f;

        Vector2 mousePos = e.mousePosition;

        if (mousePos.x > windowSize.x - edge)
            state.Pan += new Vector2(-panSpeed, 0f);
        else if (mousePos.x < edge)
            state.Pan += new Vector2(panSpeed, 0f);

        if (mousePos.y > windowSize.y - edge)
            state.Pan += new Vector2(0f, -panSpeed);
        else if (mousePos.y < edge)
            state.Pan += new Vector2(0f, panSpeed);

        GUI.changed = true;
    }

    private void HandleRightClick(Event e, GraphEditorState state, GraphRenderer renderer, Vector2 windowSize)
    {
        if (TryRemoveConnectionByClick(e.mousePosition, state))
        {
            GUI.changed = true;
            e.Use();
            return;
        }

        Vector2 graphMousePos = _viewHandler.ScreenToGraph(e.mousePosition, state);
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

    private bool TryRemoveConnectionByClick(Vector2 clickPosScreen, GraphEditorState state)
    {
        Vector2 clickPos = _viewHandler.ScreenToGraph(clickPosScreen, state);

        foreach (var node in state.Graph.Nodes)
        {
            for (int c = node.Connections.Count - 1; c >= 0; c--)
            {
                var conn = node.Connections[c];
                if (conn.Target == null) continue;

                Vector2 start = node.Position + GetNodeSize(node) / 2;
                Vector2 end = conn.Target.Position + GetNodeSize(conn.Target) / 2;

                if (IsPointNearLine(clickPos, start, end, LineClickThreshold / state.Zoom))
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

    private Vector2 GetNodeSize(GraphNodeData node)
    {
        // This must match renderer’s unscaled size
        return new Vector2(280f, 150f);
    }

    private GraphNodeData GetNodeAtPosition(Vector2 graphPos, GraphEditorState state)
    {
        foreach (var node in state.Graph.Nodes)
        {
            float h = 150f; // Approx height
            var rect = new Rect(node.Position, new Vector2(280f, h));
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

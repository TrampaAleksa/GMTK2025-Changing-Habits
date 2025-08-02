using UnityEditor;
using UnityEngine;

public class GraphInteractionHandler
{
    private const float LineClickThreshold = 10f;

    public void ProcessEvents(Event e, GraphEditorState state, GraphRenderer renderer)
    {
        if (e.type == EventType.MouseDown && e.button == 0)
            HandleLeftClick(e, state, renderer);

        if (e.type == EventType.MouseDrag && e.button == 0 && state.SelectedNode != null && !e.control)
            HandleNodeDrag(e, state);

        if (e.type == EventType.MouseDown && e.button == 1)
            HandleRightClick(e, state, renderer);
    }

    private void HandleLeftClick(Event e, GraphEditorState state, GraphRenderer renderer)
    {
        var clickedNode = GetNodeAtPosition(e.mousePosition, state, renderer);
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
        state.SelectedNode.Position += e.delta;
        GUI.changed = true;
    }

    private void HandleRightClick(Event e, GraphEditorState state, GraphRenderer renderer)
    {
        if (TryRemoveConnectionByClick(e.mousePosition, state, renderer))
        {
            GUI.changed = true;
            e.Use();
            return;
        }

        var mousePos = e.mousePosition - state.Pan;
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Node"), false, () => CreateNode(mousePos, state));
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

    private bool TryRemoveConnectionByClick(Vector2 clickPos, GraphEditorState state, GraphRenderer renderer)
    {
        foreach (var node in state.Graph.Nodes)
        {
            for (int c = node.Connections.Count - 1; c >= 0; c--)
            {
                var conn = node.Connections[c];
                if (conn.Target == null) continue;

                Vector2 start = renderer.GetNodeCenter(node, state);
                Vector2 end = renderer.GetNodeCenter(conn.Target, state);

                if (IsPointNearLine(clickPos, start, end, LineClickThreshold))
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

    private GraphNodeData GetNodeAtPosition(Vector2 pos, GraphEditorState state, GraphRenderer renderer)
    {
        foreach (var node in state.Graph.Nodes)
        {
            var rect = renderer.GetNodeRect(node, state);
            if (rect.Contains(pos))
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
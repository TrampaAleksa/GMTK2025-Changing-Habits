using UnityEngine;

public class GraphViewHandler
{
    private const float MinZoom = 0.25f;
    private const float MaxZoom = 2f;

    public void HandleViewEvents(Event e, GraphEditorState state)
    {
        HandleZoom(e, state);
        HandlePanning(e, state);
    }

    private void HandleZoom(Event e, GraphEditorState state)
    {
        if (e.type == EventType.ScrollWheel)
        {
            float zoomDelta = -e.delta.y * 0.1f;
            state.Zoom = Mathf.Clamp(state.Zoom + zoomDelta, MinZoom, MaxZoom);
            e.Use();
        }
    }

    private void HandlePanning(Event e, GraphEditorState state)
    {
        if (e.button == 2 && e.type == EventType.MouseDrag)
        {
            state.Pan += e.delta;
            e.Use();
            GUI.changed = true;
        }
    }

    // Converts graph coordinates to screen coordinates
    public Vector2 GraphToScreen(Vector2 graphPos, GraphEditorState state)
    {
        return state.Pan + graphPos * state.Zoom;
    }

    // Converts screen coordinates to graph coordinates
    public Vector2 ScreenToGraph(Vector2 screenPos, GraphEditorState state)
    {
        return (screenPos - state.Pan) / state.Zoom;
    }
}
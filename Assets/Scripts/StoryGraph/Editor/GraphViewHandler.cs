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
            state.Pan += e.delta / state.Zoom;
            e.Use();
            GUI.changed = true;
        }
    }

    public Matrix4x4 BeginView(GraphEditorState state)
    {
        Matrix4x4 oldMatrix = GUI.matrix;

        Matrix4x4 translation = Matrix4x4.TRS(state.Pan, Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(Vector3.one * state.Zoom);

        GUI.matrix = translation * scale;
        return oldMatrix;
    }

    public void EndView(Matrix4x4 oldMatrix)
    {
        GUI.matrix = oldMatrix;
    }
}
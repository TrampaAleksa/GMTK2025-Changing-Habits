using UnityEditor;
using UnityEngine;

public class GraphEditorWindow : EditorWindow
{
    private GraphEditorState state = new GraphEditorState();
    private GraphViewHandler viewHandler = new GraphViewHandler();
    private GraphRenderer renderer = new GraphRenderer();
    private GraphInteractionHandler interaction = new GraphInteractionHandler();

    [MenuItem("Window/Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<GraphEditorWindow>("Graph Editor");
    }

    private void OnGUI()
    {
        state.Graph = (GraphData)EditorGUILayout.ObjectField("Graph", state.Graph, typeof(GraphData), false);

        if (state.Graph == null)
            return;

        viewHandler.HandleViewEvents(Event.current, state);

        Matrix4x4 oldMatrix = GUI.matrix;
        viewHandler.BeginView(state);

        BeginWindows();
        renderer.DrawConnections(state);
        renderer.DrawNodes(state);
        EndWindows();

        viewHandler.EndView(oldMatrix);

        interaction.ProcessEvents(Event.current, state, renderer);

        if (GUI.changed)
            Repaint();
    }
    
    private void HandleZoom(Event e)
    {
        if (e.type == EventType.ScrollWheel)
        {
            float zoomDelta = -e.delta.y * 0.1f;
            state.Zoom = Mathf.Clamp(state.Zoom + zoomDelta, 0.25f, 2f);
            e.Use();
        }
    }
    
    private void HandlePanning(Event e)
    {
        if (e.button == 2 && e.type == EventType.MouseDrag) // Middle mouse drag
        {
            state.Pan += e.delta / state.Zoom; // scale pan according to zoom
            e.Use();
            GUI.changed = true;
        }
    }


}

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

    public void BeginView(GraphEditorState state)
    {
        GUIUtility.ScaleAroundPivot(Vector2.one * state.Zoom, Vector2.zero);
    }

    public void EndView(Matrix4x4 oldMatrix)
    {
        GUI.matrix = oldMatrix;
    }
}

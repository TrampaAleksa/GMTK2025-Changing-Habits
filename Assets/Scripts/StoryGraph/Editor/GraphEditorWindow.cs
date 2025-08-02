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

        renderer.DrawConnections(viewHandler, state);
        renderer.DrawNodes(viewHandler, state);

        interaction.ProcessEvents(viewHandler, Event.current, state, renderer, position.size);

        if (GUI.changed)
            Repaint();
    }

}
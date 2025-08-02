using UnityEditor;
using UnityEngine;

public class GraphEditorWindow : EditorWindow
{
    private GraphEditorState state = new GraphEditorState();
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

        BeginWindows();
        renderer.DrawConnections(state);
        renderer.DrawNodes(state);
        EndWindows();

        interaction.ProcessEvents(Event.current, state, renderer);

        if (GUI.changed)
            Repaint();
    }
}
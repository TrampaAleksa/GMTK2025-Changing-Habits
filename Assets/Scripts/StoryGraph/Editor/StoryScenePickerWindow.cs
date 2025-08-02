using System.Linq;
using UnityEditor;
using UnityEngine;

public class StoryScenePickerWindow : EditorWindow
{
    private GameObject[] validPrefabs;
    private Vector2 scrollPos;
    private System.Action<StoryScene> onPicked;

    public static void Show(System.Action<StoryScene> onPicked)
    {
        var window = CreateInstance<StoryScenePickerWindow>();
        window.titleContent = new GUIContent("Select StoryScene Prefab");
        window.minSize = new Vector2(300, 400);
        window.onPicked = onPicked;
        window.Refresh();
        window.ShowUtility();
    }

    private void Refresh()
    {
        // Find all prefabs with a StoryScene component in the project
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        validPrefabs = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath((string)g)))
            .Where(go => go != null && go.GetComponent<StoryScene>() != null)
            .ToArray();
    }

    private void OnGUI()
    {
        if (validPrefabs == null || validPrefabs.Length == 0)
        {
            EditorGUILayout.LabelField("No StoryScene prefabs found.");
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var prefab in validPrefabs)
        {
            if (GUILayout.Button(prefab.name, GUILayout.Height(24)))
            {
                var scene = prefab.GetComponent<StoryScene>();
                onPicked?.Invoke(scene);
                Close();
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
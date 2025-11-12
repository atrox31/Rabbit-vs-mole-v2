using UnityEditor;
using UnityEngine;

namespace DialogueSystem.Editor
{
    // Tells Unity to use this custom editor when a DialogueSequence asset is selected
    [CustomEditor(typeof(DialogueSequence))]
    public class DialogueSequenceEditor : UnityEditor.Editor
    {
        // This method is called when the ScriptableObject is double-clicked
        public override void OnInspectorGUI()
        {
            // Draw the default inspector (optional, but often useful)
            DrawDefaultInspector();

            // Cast the target object to our specific type
            DialogueSequence graph = (DialogueSequence)target;

            // Add a button to open the graph editor window
            if (GUILayout.Button("Open Dialogue Graph Editor"))
            {
                OpenEditorWindow(graph);
            }
        }

        // Method called from the menu item/button to open the editor
        public static void OpenEditorWindow(DialogueSequence graph)
        {
            // 1. Get the existing window instance or create a new one
            DialogueGraphEditor window = EditorWindow.GetWindow<DialogueGraphEditor>();
            window.titleContent = new GUIContent("Dialogue Graph Editor: " + graph.name);

            // 2. Set the target graph for the editor window
            window.SetTarget(graph);

            // 3. Optional: Focus the window
            window.Focus();
        }
    }
}
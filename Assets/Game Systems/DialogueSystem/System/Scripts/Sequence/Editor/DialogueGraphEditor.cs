using DialogueSystem.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    public class DialogueGraphEditor : EditorWindow
    {
        private DialogueSequence _currentGraph;
        private DialogueGraphView _graphView; // Now store the graph view instance
        private bool _isDirty = false;
        private SerializedObject _serializedObject;
        public SerializedObject CurrentGraphSerializedObject => _serializedObject;


        public new void SetDirty()
        {
            if (_isDirty) return;

            _isDirty = true;
            UpdateTitle();

            if (_currentGraph != null)
            {
                EditorUtility.SetDirty(_currentGraph);
            }
        }

        [MenuItem("Tools/Dialogue System/Open Editor Window")]
        public static void Open()
        {
            GetWindow<DialogueGraphEditor>().Show();
        }

        private void UpdateTitle()
        {
            string title = "Dialogue Graph: ";
            if (_currentGraph != null)
            {
                title += _currentGraph.name;
                if (_isDirty)
                {
                    title += "*";
                }
            }
            else
            {
                title += " (No Graph Loaded)";
            }

            titleContent = new GUIContent(title);
        }
        public SerializedProperty GetNodeProperty(DialogueNode nodeData)
        {
            if (_currentGraph == null || _serializedObject == null) return null;

            var nodesProperty = _serializedObject.FindProperty("Nodes");

            if (nodesProperty == null)
            {
                Debug.LogError($"Property 'Nodes' not found in DialogueSequence. Check the field name.");
                return null;
            }

            if (!nodesProperty.isArray)
            {
                Debug.LogError($"Property 'Nodes' is not an array/list. Check the field type.");
                return null;
            }

            // Krok 2: Wyszukujemy w³aœciwoœæ.
            for (int i = 0; i < nodesProperty.arraySize; i++)
            {
                var element = nodesProperty.GetArrayElementAtIndex(i);
                if (element == null) continue;

                SerializedProperty guidProperty = element.FindProperty("GUID");
                if (guidProperty != null && guidProperty.stringValue == nodeData.GUID)
                {
                    return element;
                }
            }
            return null;
        }

        public void CreateGUI()
        {
            // 1. Setup GraphView
            _graphView = new DialogueGraphView(this);
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);

            // 2. Setup Toolbar
            var toolbar = new Toolbar();

            // Button to create a new node at the center of the current view
            Button newNodeButton = new Button(() => _graphView.CreateNode(new Vector2(100, 100)));
            newNodeButton.text = "New Dialogue Node";
            toolbar.Add(newNodeButton);

            // Save Button
            Button saveButton = new Button(SaveGraph) { text = "Save Graph" };
            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);

            // Load the last selected graph if the window was previously open
            if (_currentGraph != null)
            {
                LoadGraph(_currentGraph);
            }
        }

        public void SetTarget(DialogueSequence graph)
        {
            // If the graph is the same, do nothing
            if (_currentGraph == graph) return;

            _currentGraph = graph;
            titleContent = new GUIContent("Dialogue Graph: " + graph.name);

            if (_currentGraph != null)
            {
                _serializedObject = new SerializedObject(_currentGraph);
            }
            else
            {
                _serializedObject = null;
            }

            // If the GUI is already built, load the graph immediately
            if (_graphView != null)
            {
                LoadGraph(graph);
            }
        }

        private void LoadGraph(DialogueSequence graph)
        {
            // Clear the existing elements from the view
            _graphView.ClearGraph();

            // Dictionary for fast access to node views by GUID
            var nodeViewMap = new Dictionary<string, DialogueNodeView>();

            foreach (var nodeData in graph.Nodes)
            {
                var nodeView = new DialogueNodeView(nodeData, this);
                _graphView.AddElement(nodeView);
                nodeViewMap.Add(nodeData.GUID, nodeView);
            }

            foreach (var nodeData in graph.Nodes)
            {
                // Source node (the one that has an exit)
                if (!nodeViewMap.TryGetValue(nodeData.GUID, out var sourceNodeView)) continue;

                foreach (var link in nodeData.ExitPorts)
                {
                    // Target node (the one the connection points to)
                    if (nodeViewMap.TryGetValue(link.TargetNodeGUID, out var targetNodeView))
                    {
                        // Use a utility method to create and connect the edge visually
                        Edge edge = sourceNodeView.OutputPort.ConnectTo(targetNodeView.InputPort);
                        _graphView.AddElement(edge);
                    }
                    else
                    {
                        Debug.LogWarning($"Target node with GUID {link.TargetNodeGUID} not found during graph loading.");
                    }
                }
            }
            _isDirty = false;
        }

        private void SaveGraph()
        {
            if (_currentGraph == null || _serializedObject == null) return;
            
            _serializedObject.ApplyModifiedProperties();
            var serializedObject = new SerializedObject(_currentGraph);
            serializedObject.ApplyModifiedProperties();

            // Clear all existing connections in the ScriptableObject data 
            // to save only the current ones.
            _currentGraph.Nodes.ForEach(node => node.ExitPorts.Clear());

            // Save Edges (Connections)
            var edges = _graphView.edges.ToList(); 

            foreach (var edge in edges)
            {
                // Find the source and target nodes based on their ports
                var sourceNodeView = edge.output.node as DialogueNodeView;
                var targetNodeView = edge.input.node as DialogueNodeView;

                if (sourceNodeView != null && targetNodeView != null)
                {
                    // Create a new link object
                    var link = new NodeLink
                    {
                        TargetNodeGUID = targetNodeView.NodeData.GUID
                    };

                    // Add the link to the source node's exit ports list
                    sourceNodeView.NodeData.ExitPorts.Add(link);
                }
            }

            // Mark the asset as dirty to ensure it's saved to disk
            EditorUtility.SetDirty(_currentGraph);

            // Save all pending changes to the asset
            AssetDatabase.SaveAssets();

            _isDirty = false;
            UpdateTitle();

            Debug.Log("Dialogue Graph saved successfully.");
        }
        private void OnDisable()
        {
            if (_isDirty)
            {
                // Wyœwietl okno dialogowe z ostrze¿eniem
                bool saveChanges = EditorUtility.DisplayDialog(
                    "Unsaved Changes",
                    "The Dialogue Graph has unsaved changes. Do you want to save them before closing?",
                    "Save and Close",  
                    "Discard Changes"
                );

                if (saveChanges)
                {
                    SaveGraph();
                }
            }
        }

        public void AddNodeToGraph(DialogueNode nodeData)
        {
            if (_currentGraph == null)
            {
                Debug.LogError("Cannot add node: No DialogueSequence asset is currently selected.");
                return;
            }
            // Add the new data to the list in the ScriptableObject
            _currentGraph.Nodes.Add(nodeData);

            // If it's the first node, set it as the start node
            if (_currentGraph.Nodes.Count == 1)
            {
                _currentGraph.StartNodeGUID = nodeData.GUID;
            }

            if (_serializedObject != null)
            {
                _serializedObject.Update();
            }

            SaveGraph();
        }
    }
}
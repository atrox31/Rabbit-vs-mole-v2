using DialogueSystem.Nodes;
using DialogueSystem;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace DialogueSystem.Editor
{
    /// <summary>
    /// Visual representation of a TriggerNode in the graph editor.
    /// </summary>
    public class TriggerNodeView : Node
    {
        public TriggerNode NodeData;
        public Port InputPort;
        public Port OutputPort;

        private ObjectField _triggerField;
        private DialogueGraphEditor _editor;
        private SerializedProperty _nodeProperty;
        private bool _isLoading = true;

        public TriggerNodeView(TriggerNode nodeData, DialogueGraphEditor editor)
        {
            NodeData = nodeData;
            _editor = editor;
            title = "Trigger Node";
            
            // Set node color to distinguish from dialogue nodes
            titleContainer.style.backgroundColor = new Color(0.3f, 0.6f, 0.3f, 0.8f);

            // Set the position using the data from the ScriptableObject
            SetPosition(new Rect(NodeData.EditorPosition, new Vector2(300, 150)));

            CreateInputPorts();
            CreateOutputPorts();
            SetupCustomDataFields();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            
            // Mark loading as complete after a frame
            schedule.Execute(() => { _isLoading = false; });
        }

        private void CreateInputPorts()
        {
            InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);
        }

        private void CreateOutputPorts()
        {
            OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Next";
            outputContainer.Add(OutputPort);
        }

        private void SetupCustomDataFields()
        {
            // Get SerializedProperty for this node
            _nodeProperty = _editor.GetTriggerNodeProperty(NodeData);
            if (_nodeProperty == null)
            {
                Debug.LogError("Failed to find SerializedProperty for TriggerNode.");
                return;
            }

            // Update SerializedProperty before reading values
            _nodeProperty.serializedObject.Update();

            // Trigger Field - can be MonoBehaviour or ScriptableObject implementing IDialogueTrigger
            SerializedProperty triggerComponentProperty = _nodeProperty.FindProperty("_triggerComponent");
            SerializedProperty triggerSOProperty = _nodeProperty.FindProperty("_triggerScriptableObject");

            // Try to get current value
            Object currentTrigger = null;
            if (triggerComponentProperty != null && triggerComponentProperty.objectReferenceValue != null)
            {
                currentTrigger = triggerComponentProperty.objectReferenceValue as Object;
            }
            else if (triggerSOProperty != null && triggerSOProperty.objectReferenceValue != null)
            {
                currentTrigger = triggerSOProperty.objectReferenceValue as Object;
            }

            _triggerField = new ObjectField("Trigger:")
            {
                objectType = typeof(Object), // Accept any Object (MonoBehaviour or ScriptableObject)
                allowSceneObjects = true,
                value = currentTrigger
            };

            _triggerField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null)
                {
                    // Clear both properties
                    if (triggerComponentProperty != null)
                        triggerComponentProperty.objectReferenceValue = null;
                    if (triggerSOProperty != null)
                        triggerSOProperty.objectReferenceValue = null;
                }
                else if (evt.newValue is MonoBehaviour mb && mb is IDialogueTrigger)
                {
                    // Set as MonoBehaviour component
                    if (triggerComponentProperty != null)
                    {
                        triggerComponentProperty.objectReferenceValue = mb;
                    }
                    if (triggerSOProperty != null)
                    {
                        triggerSOProperty.objectReferenceValue = null;
                    }
                    NodeData.SetTriggerComponent(mb);
                }
                else if (evt.newValue is ScriptableObject so && so is IDialogueTrigger)
                {
                    // Set as ScriptableObject
                    if (triggerSOProperty != null)
                    {
                        triggerSOProperty.objectReferenceValue = so;
                    }
                    if (triggerComponentProperty != null)
                    {
                        triggerComponentProperty.objectReferenceValue = null;
                    }
                    NodeData.SetTriggerScriptableObject(so);
                }
                else
                {
                    Debug.LogWarning($"Selected object does not implement IDialogueTrigger. Type: {evt.newValue.GetType()}");
                    return;
                }

                if (_nodeProperty != null)
                {
                    _nodeProperty.serializedObject.ApplyModifiedProperties();
                }
                _editor.SetDirty();
            });

            extensionContainer.Add(_triggerField);
            RefreshPorts();
            RefreshExpandedState();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (_isLoading) return;

            Vector2 newPosition = GetPosition().position;
            NodeData.EditorPosition = newPosition;

            if (_nodeProperty != null)
            {
                SerializedProperty positionProperty = _nodeProperty.FindProperty("EditorPosition");
                if (positionProperty != null)
                {
                    positionProperty.vector2Value = newPosition;
                    positionProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            _editor.SetDirty();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction(
                "Duplicate",
                (action) => DuplicateNode(),
                DropdownMenuAction.AlwaysEnabled
            );

            evt.menu.AppendAction(
                "Delete",
                (action) => DeleteNode(),
                DropdownMenuAction.AlwaysEnabled
            );
        }

        private void DuplicateNode()
        {
            if (_editor == null || _editor.CurrentGraph == null) return;

            var graphView = _editor.GetGraphView();
            if (graphView == null) return;

            Vector2 newPosition = NodeData.EditorPosition + new Vector2(50, 50);
            var newNodeData = new TriggerNode(newPosition);

            // Copy trigger reference
            var trigger = NodeData.GetTrigger();
            if (trigger is MonoBehaviour mb)
            {
                newNodeData.SetTriggerComponent(mb);
            }
            else if (trigger is ScriptableObject so)
            {
                newNodeData.SetTriggerScriptableObject(so);
            }

            _editor.CurrentGraph.TriggerNodes.Add(newNodeData);

            var newNodeView = new TriggerNodeView(newNodeData, _editor);
            graphView.AddElement(newNodeView);

            _editor.SetDirty();
            Debug.Log("Trigger node duplicated.");
        }

        private void DeleteNode()
        {
            if (_editor == null || _editor.CurrentGraph == null) return;

            var graphView = _editor.GetGraphView();
            if (graphView == null) return;

            var edgesToRemove = graphView.edges.Where(e =>
                e.output.node == this || e.input.node == this).ToList();

            foreach (var edge in edgesToRemove)
            {
                graphView.RemoveElement(edge);
            }

            _editor.CurrentGraph.TriggerNodes.Remove(NodeData);
            graphView.RemoveElement(this);

            _editor.SetDirty();
            Debug.Log("Trigger node deleted.");
        }
    }
}


// DialogueNodeView.cs (w Assets/Editor)
using DialogueSystem.Nodes;
using DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor
{
    // Custom Node class for the visual representation of DialogueNode data
    public class DialogueNodeView : Node
    {
        public DialogueNode NodeData;
        public Port InputPort;
        public Port OutputPort;

        private TextField _dialogueText;
        private ObjectField _actorField;
        private DropdownField _poseDropdown;
        private EnumField _sideField;
        private DialogueGraphEditor _editor;

        public DialogueNodeView(DialogueNode nodeData, DialogueGraphEditor editor)
        {
            NodeData = nodeData;
            _editor = editor;
            title = "Dialogue Node";

            // Set the position using the data from the ScriptableObject
            SetPosition(new Rect(NodeData.EditorPosition, new Vector2(300, 200)));

            CreateInputPorts();
            CreateOutputPorts();
            SetupCustomDataFields();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void CreateInputPorts()
        {
            // Always create one input port (previous node)
            InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);
        }

        private void CreateOutputPorts()
        {
            // For now, only one output port (for simple dialogue flow)
            OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Next";
            outputContainer.Add(OutputPort);

            //   TODO: przerobiæ na ró¿ne opcje z warunkami jakimiœ najlpeijej              /\
            //                                trza bedzie zmieniæ ¿eby da³o siê ró¿ne robiæ ||
        }

        private void SetupCustomDataFields()
        {
            // for pose property
            SerializedProperty nodeSerializedProperty = _editor.GetNodeProperty(NodeData);
            if (nodeSerializedProperty == null)
            {
                Debug.LogError("Failed to find SerializedProperty for DialogueNode.");
                return;
            }
            SerializedProperty poseProperty = nodeSerializedProperty.FindProperty("_poseName");
            if (poseProperty == null)
            {
                Debug.LogError("Failed to find _poseName property.");
                return;
            }

            // 1. Actor Field (ObjectField for your Actor ScriptableObject)
            _actorField = new ObjectField("Actor:")
            {
                objectType = typeof(Actor), // Assuming 'Actor' is a ScriptableObject/class
                allowSceneObjects = false,
                value = NodeData._actor
            };
            _actorField.RegisterValueChangedCallback(evt =>
            {
                NodeData._actor = evt.newValue as Actor;
                _editor.SetDirty();
                if (_editor.CurrentGraphSerializedObject != null)
                {
                    _editor.CurrentGraphSerializedObject.Update(); // Pobierz najnowsze dane
                }
                UpdatePoseDropdownOptions();
            });

            // 2. Pose Field
            _poseDropdown = new DropdownField("Pose:");
            _poseDropdown.style.width = 250; 
            UpdatePoseDropdownOptions();

            _poseDropdown.RegisterValueChangedCallback(evt =>
            {
                // Aktualizujemy SerializedProperty (pole _poseName)
                poseProperty.stringValue = evt.newValue;
                poseProperty.serializedObject.ApplyModifiedProperties();

                _editor.SetDirty();
            });

            // 3. Screen Position Field (EnumField for the Side enum)
            _sideField = new EnumField("Screen Position:", ((ActorSideOnScreen)0).GetFirstValue()) // Default value or read from NodeData
            {
                value = NodeData.ScreenPosition
            };
            _sideField.RegisterValueChangedCallback(evt =>
            {
                NodeData.ScreenPosition = (DialogueSystem.ActorSideOnScreen)evt.newValue;
                _editor.SetDirty();
            });

            // 4. Dialogue Text Field
            _dialogueText = new TextField("Dialogue:", -1,true, false, '\0')
            {
                value = NodeData.text ?? "New dialogue text...",
                multiline = true
            };
            _dialogueText.style.minHeight = 80;
            _dialogueText.style.width = 250;
            _dialogueText.style.whiteSpace = WhiteSpace.Normal;
            _dialogueText.Q<TextElement>().style.whiteSpace = WhiteSpace.Normal;

            _dialogueText.RegisterValueChangedCallback(evt =>
            {
                NodeData.text = evt.newValue;
                _editor.SetDirty();
            });

            // Add fields to the node's main body
            extensionContainer.Add(_actorField); 
            extensionContainer.Add(_poseDropdown); 
            extensionContainer.Add(_sideField);
            extensionContainer.Add(_dialogueText);

            RefreshPorts();
            RefreshExpandedState();
        }
        private void UpdatePoseDropdownOptions()
        {
            List<string> poseNames = new List<string> { "(No Pose Selected)" };
            string currentValue = NodeData._poseName;

            if (NodeData._actor != null && NodeData._actor.poses != null)
            {
                // Add actual pose names
                poseNames.AddRange(NodeData._actor.poses.Select(p => p.name));
            }
            _poseDropdown.choices = poseNames;
            if (poseNames.Contains(currentValue))
            {
                _poseDropdown.value = currentValue;
            }
            else
            {
                _poseDropdown.value = poseNames[0];
                NodeData._poseName = "";
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // The new position is stored in the newRect of the GeometryChangedEvent
            NodeData.EditorPosition = GetPosition().position;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Nodes
{
    /// <summary>
    /// Node type that executes a trigger during dialogue sequence.
    /// </summary>
    [Serializable]
    public class TriggerNode
    {
        // GUID used by GraphView/Editor to uniquely identify this node
        public string GUID;

        // Position used by GraphView/Editor to draw the node on screen
        public Vector2 EditorPosition;

        // --- TRIGGER CONTENT ---
        [SerializeField] private MonoBehaviour _triggerComponent;
        [SerializeField] private ScriptableObject _triggerScriptableObject;

        // --- CONNECTION LOGIC ---
        public List<NodeLink> ExitPorts = new List<NodeLink>();

        // Constructor for editor utility
        public TriggerNode(Vector2 position)
        {
            GUID = Guid.NewGuid().ToString();
            EditorPosition = position;
        }

        /// <summary>
        /// Gets the trigger to execute. Returns MonoBehaviour if set, otherwise ScriptableObject.
        /// </summary>
        public IDialogueTrigger GetTrigger()
        {
            if (_triggerComponent != null && _triggerComponent is IDialogueTrigger triggerMono)
            {
                return triggerMono;
            }
            
            if (_triggerScriptableObject != null && _triggerScriptableObject is IDialogueTrigger triggerSO)
            {
                return triggerSO;
            }

            return null;
        }

        /// <summary>
        /// Sets the trigger component (MonoBehaviour).
        /// </summary>
        public void SetTriggerComponent(MonoBehaviour component)
        {
            _triggerComponent = component;
            _triggerScriptableObject = null;
        }

        /// <summary>
        /// Sets the trigger scriptable object.
        /// </summary>
        public void SetTriggerScriptableObject(ScriptableObject scriptableObject)
        {
            _triggerScriptableObject = scriptableObject;
            _triggerComponent = null;
        }

        public override string ToString()
        {
            var trigger = GetTrigger();
            return $"[TriggerNode] Trigger: {(trigger != null ? trigger.GetType().Name : "None")}";
        }
    }
}


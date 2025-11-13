using DialogueSystem.Nodes;
using DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Create Dialogue Graph")]
    public class DialogueSequence : ScriptableObject
    {
        // This list is serialized by Unity (ScriptableObject) and will be filled by GraphView.
        [HideInInspector]
        public List<DialogueNode> Nodes = new List<DialogueNode>();

        // Trigger nodes list
        [HideInInspector]
        public List<TriggerNode> TriggerNodes = new List<TriggerNode>();

        // GUID of the first node where the conversation starts.
        public string StartNodeGUID;

        // Runtime Dictionary for fast lookup (populated in Awake/OnEnable)
        private Dictionary<string, DialogueNode> _nodeMap;
        private Dictionary<string, TriggerNode> _triggerNodeMap;

        public Dictionary<string, DialogueNode> NodeMap
        {
            get
            {
                if (_nodeMap == null || _nodeMap.Count != Nodes.Count)
                {
                    _nodeMap = Nodes.ToDictionary(node => node.GUID);
                }
                return _nodeMap;
            }
        }

        public Dictionary<string, TriggerNode> TriggerNodeMap
        {
            get
            {
                if (_triggerNodeMap == null || _triggerNodeMap.Count != TriggerNodes.Count)
                {
                    _triggerNodeMap = TriggerNodes.ToDictionary(node => node.GUID);
                }
                return _triggerNodeMap;
            }
        }

        /// <summary>
        /// Gets a node by GUID, checking both DialogueNodes and TriggerNodes.
        /// </summary>
        public object GetNodeByGUID(string guid)
        {
            if (NodeMap.ContainsKey(guid))
                return NodeMap[guid];
            if (TriggerNodeMap.ContainsKey(guid))
                return TriggerNodeMap[guid];
            return null;
        }

        /// <summary>
        /// Checks if a GUID belongs to a DialogueNode.
        /// </summary>
        public bool IsDialogueNode(string guid)
        {
            return NodeMap.ContainsKey(guid);
        }

        /// <summary>
        /// Checks if a GUID belongs to a TriggerNode.
        /// </summary>
        public bool IsTriggerNode(string guid)
        {
            return TriggerNodeMap.ContainsKey(guid);
        }
    }
}
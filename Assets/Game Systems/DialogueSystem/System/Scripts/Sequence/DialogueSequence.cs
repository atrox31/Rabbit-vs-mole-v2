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

        // GUID of the first node where the conversation starts.
        public string StartNodeGUID;

        // Runtime Dictionary for fast lookup (populated in Awake/OnEnable)
        private Dictionary<string, DialogueNode> _nodeMap;

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
    }
}
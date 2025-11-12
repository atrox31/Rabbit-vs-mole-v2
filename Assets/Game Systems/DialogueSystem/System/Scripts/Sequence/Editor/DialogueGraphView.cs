using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Linq;

namespace DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        private DialogueGraphEditor _editor;

        public DialogueGraphView(DialogueGraphEditor editor)
        {
            _editor = editor;

            // Setup panning, zooming
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Add grid background
            this.Insert(0, new GridBackground());

            // Add manipulation abilities (selection, dragging)
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
        }

        // Defines which ports can be connected
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction && // Input to Output or vice versa
                endPort.node != startPort.node).ToList();    // Not connecting to itself
        }

        // Method to create a new, generic node for testing
        public void CreateNode(Vector2 position)
        {
            var nodeData = new DialogueSystem.Nodes.DialogueNode(position);
            _editor.AddNodeToGraph(nodeData);
            var nodeView = new DialogueNodeView(nodeData, _editor);

            AddElement(nodeView);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Reset the default menu before adding our custom items
            evt.menu.MenuItems().Clear();

            // Get the position where the user right-clicked (in world space)
            Vector2 screenMousePosition = evt.localMousePosition;

            // Convert the screen position to the GraphView's local content position
            Vector2 worldMousePosition = contentViewContainer.WorldToLocal(screenMousePosition);

            evt.menu.AppendAction(
                "Create",
                (action) => CreateNode(worldMousePosition),
                DropdownMenuAction.AlwaysEnabled
            );

            evt.menu.AppendSeparator();

            // default options
            //base.BuildContextualMenu(evt);
        }
        
        public void ClearGraph()
        {
            // Remove all nodes and edges
            DeleteElements(graphElements.Where(elem => elem is Node || elem is Edge));
        }
    }
}
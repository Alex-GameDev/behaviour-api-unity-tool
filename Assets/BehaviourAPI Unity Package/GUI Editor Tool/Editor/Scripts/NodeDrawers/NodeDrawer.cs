using System;
using UnityEditor.Experimental.GraphView;

namespace BehaviourAPI.Unity.Editor
{
    using Core;
    using UnityEngine.UIElements;

    public abstract class NodeDrawer
    {
        protected MNodeView view;
        protected Node node;

        public abstract string LayoutPath { get; }

        public void SetView(MNodeView nodeView, Node node)
        {
            this.view = nodeView;
            this.node = node;
        }

        public abstract void SetUpPorts();
        public abstract void DrawNodeDetails();

        public abstract PortView GetPort(MNodeView nodeView, Direction direction);

        #region --------------------------------------------- Events ---------------------------------------------

        /// <summary>
        /// Add options to the node contextual menu.
        /// </summary>
        /// <param name="evt">The contextual menu creation event.</param>
        public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        /// <summary>
        /// Method called when the node needs to be repainted.
        /// </summary>
        public virtual void OnRepaint() { }

        /// <summary>
        /// Method called when the node is selected.
        /// </summary>
        public virtual void OnSelected() { }

        /// <summary>
        /// Method called when the node is unselected.
        /// </summary>
        public virtual void OnUnselected() {}

        /// <summary>
        /// Method called when a new connection is created in the node.
        /// </summary>
        public virtual void OnConnected() { }

        /// <summary>
        /// Method called when a new connection is deleted in the node.
        /// </summary>
        public virtual void OnDisconnected() { }

        /// <summary>
        /// Method called when the node is being removed from the graph.
        /// </summary>
        public virtual void OnDeleted() { }

        #endregion

        public static NodeDrawer Create(Node node)
        {
            if (BehaviourAPISettings.instance.Metadata.NodeDrawerTypeMap.TryGetValue(node.GetType(), out Type drawerType))
            {
                return (NodeDrawer)Activator.CreateInstance(drawerType);
            }
            else
            {
                return new BTNodeDrawer();
            }
          
        }


    }
}

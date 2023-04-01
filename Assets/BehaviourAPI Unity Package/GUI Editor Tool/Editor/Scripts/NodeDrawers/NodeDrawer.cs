using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace BehaviourAPI.Unity.Editor
{
    using Core;
    public abstract class NodeDrawer
    {
        protected MNodeView nodeView;
        protected Node node;

        public abstract string LayoutPath { get; }

        public void SetView(MNodeView nodeView, Node node)
        {
            this.nodeView = nodeView;
            this.node = node;
        }

        public abstract void SetUpPorts();
        public abstract void DrawNodeDetails();
        public abstract void OnRepaint();
        public abstract void OnSelected();
        public abstract void OnUnselected();
        public abstract PortView GetPort(MNodeView nodeView, Direction direction);

        internal static NodeDrawer Create(Node node)
        {
            Type drawerType = BehaviourAPISettings.instance.APITypeMetadata.NodeDrawerTypeMap.GetValueOrDefault(node.GetType());

            if(drawerType != null)
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

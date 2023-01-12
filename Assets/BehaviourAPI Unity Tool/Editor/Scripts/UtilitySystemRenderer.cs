using BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UtilitySystems;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(UtilitySystem))]
    public class UtilitySystemRenderer : GraphRenderer
    {
        string selectableLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);
        string factorLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public override void DrawConnections(NodeAsset asset)
        {
            foreach (NodeAsset child in asset.Childs)
            {
                Port srcPort = assetViewPairs[asset].OutputPort;
                Port tgtPort = assetViewPairs[child].InputPort;
                Edge edge = srcPort.ConnectTo(tgtPort);

                graphView.AddConnectionView(edge);
                srcPort.node.RefreshPorts();
                tgtPort.node.RefreshPorts();
            }
        }

        public override void DrawGraph(GraphAsset graphAsset)
        {

        }

        public override NodeView DrawNode(NodeAsset asset)
        {
            // Crear nodo
            var nodeView = new NodeView(asset, graphView, selectableLayout);

            if (graphView.Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
                nodeView.capabilities -= Capabilities.Movable;
            }

            assetViewPairs.Add(asset, nodeView);
            graphView.AddNodeView(nodeView);
            return nodeView;
        }

        public override List<Port> GetValidPorts(UQueryState<Port> ports, Port startPort)
        {
            List<Port> validPorts = new List<Port>();
            var startPortNodeView = (NodeView)startPort.node;

            var childs = startPortNodeView.Node.GetPathToLeaves();
            var parents = startPortNodeView.Node.GetPathFromRoot();

            ports.ForEach(port =>
            {
                if (startPort.direction == port.direction) return; // Same port direction
                if (startPort.node == port.node) return; // Same node

                var portNodeView = (NodeView)port.node;
                if (portNodeView == null) return;

                if (startPort.direction == Direction.Input)
                {
                    if (!port.portType.IsAssignableFrom(startPort.portType)) return;
                    if (childs.Contains(portNodeView.Node)) return;
                }
                else
                {
                    if (!startPort.portType.IsAssignableFrom(port.portType)) return;
                    if (parents.Contains(portNodeView.Node)) return;
                }

                validPorts.Add(port);
            });
            return validPorts;
        }

        public override GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            return change;
        }
    }
}

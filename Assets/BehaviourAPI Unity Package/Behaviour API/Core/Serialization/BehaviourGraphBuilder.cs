using System.Collections.Generic;

namespace BehaviourAPI.Core.Serialization
{
    public class BehaviourGraphBuilder
    {
        BehaviourGraph graph;

        List<NodeData> nodes;

        public BehaviourGraphBuilder(BehaviourGraph graph)
        {
            this.graph = graph;
            nodes = new List<NodeData>();
        }

        public void AddNode(string name, Node node, List<Node> parents, List<Node> children)
        {
            graph.AddNode(name, node);
            nodes.Add(new NodeData(node, parents, children));
        }

        public void AddNode(Node node, List<Node> parents, List<Node> children)
        {
            graph.AddNode(node);
            nodes.Add(new NodeData(node, parents, children));
        }

        public void Build()
        {
            nodes.ForEach(n => n.Build());
        }

        private struct NodeData
        {
            public Node node;
            public List<Node> parents;
            public List<Node> children;

            public NodeData(Node node, List<Node> parents, List<Node> children)
            {
                this.node = node;
                this.parents = parents;
                this.children = children;
            }

            public void Build() => node.BuildConnections(parents, children);
        }
    }
}

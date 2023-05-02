using System;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    /// <summary>
    /// Attribute indicating that the annotated type replaces another in the editor tool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAdapterAttribute : Attribute
    {
        public Type NodeType { get; private set; }

        public bool Inherited { get; private set; } = true;

        public NodeAdapterAttribute(Type nodeType)
        {
            NodeType = nodeType;
        }

        public NodeAdapterAttribute(Type nodeType, bool inherited)
        {
            NodeType = nodeType;
            Inherited = inherited;
        }
    }
}

using System.Collections.Generic;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;    

    /// <summary>
    /// BTNode that alters the result returned by its child node or its execution.
    /// </summary>
    public abstract class DecoratorNode : BTNode
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public sealed override int MaxOutputConnections => 1;

        protected BTNode m_childNode;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------
        public void SetChild(BTNode child)
        {
            if (child != null) m_childNode = child;
            else throw new MissingChildException(this, "Can't set null node as child");
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);

            if (children.Count > 0 && children[0] is BTNode bTNode)
                m_childNode = bTNode;
            else
                throw new MissingChildException(this, $"Child {children[0]} is not BTNode");
        }

        #endregion
    }
}
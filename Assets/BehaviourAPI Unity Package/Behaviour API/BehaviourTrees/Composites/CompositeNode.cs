using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// BTNode type that has multiple children and executes them according to certain conditions.
    /// </summary>
    public abstract class CompositeNode : BTNode
    {
        private static Random rng = new Random();

        #region ------------------------------------------ Properties -----------------------------------------

        public sealed override int MaxOutputConnections => -1;

        protected List<BTNode> m_children = new List<BTNode>();

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public bool IsRandomized;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public void AddChild(BTNode child)
        {
            if(child != null) m_children.Add(child);
            else throw new MissingChildException(this, "Can't add null node as child");
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);

            m_children = new List<BTNode>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is BTNode t)
                    m_children.Add(t);
                else
                    throw new MissingChildException(this, $"child {i} is not BTNode");
            }
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            base.Start();

            if (m_children.Count == 0) throw new MissingChildException(this, "This composite has no childs");

            if (IsRandomized) m_children = m_children.OrderBy(elem => rng.NextDouble()).ToList();
        }

        public override void Stop()
        {
            base.Stop();

            if (m_children.Count == 0) throw new MissingChildException(this, "This composite has no childs");
        }

        protected BTNode GetBTChildAt(int idx)
        {
            if (m_children.Count == 0) throw new MissingChildException(this, "This composite has no childs");
            if (idx < 0 || idx >= m_children.Count) throw new MissingChildException(this, "This composite has no child at index " + idx);
            
            return m_children[idx];
        }

        protected int BTChildCount => m_children.Count;

        #endregion
    }
}
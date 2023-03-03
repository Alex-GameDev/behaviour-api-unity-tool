using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.UtilitySystems
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// Factor that combines the utility of its childs factors.
    /// </summary>
    public abstract class FusionFactor : Factor
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public override int MaxOutputConnections => -1;

        protected List<Factor> m_childFactors;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public FusionFactor()
        {
            m_childFactors = new List<Factor>();
        }

        public void AddFactor(Factor factor)
        {
            if (factor != null) m_childFactors.Add(factor);
            else throw new MissingChildException(this, "Can't add null node as child");
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);

            m_childFactors = new List<Factor>();
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Factor f)
                    m_childFactors.Add(f);
                else
                    throw new MissingChildException(this, $"Child {i} node is not a Factor");
            }
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        protected override float ComputeUtility()
        {
            m_childFactors.ForEach(f => f.UpdateUtility());
            return Evaluate(m_childFactors.Select(child => child.Utility).ToList());
        }

        protected abstract float Evaluate(List<float> utilities);

        #endregion
    }
}

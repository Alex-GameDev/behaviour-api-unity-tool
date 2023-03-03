using System.Collections.Generic;

namespace BehaviourAPI.UtilitySystems
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// Factor that modifies its child value with a function.
    /// </summary>  
    public abstract class CurveFactor : Factor
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public override int MaxOutputConnections => 1;

        Factor m_childFactor;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public void SetChild(Factor factor)
        {
            if(factor != null)
            {
                m_childFactor = factor;
            }
            else
            {
                throw new MissingChildException(this, "Can't set null node as child");
            }
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);

            if (children.Count > 0 && children[0] is Factor factor)
                m_childFactor = factor;
            else
                throw new MissingChildException(this, "This function factor has no child, or it's type is incorrect.");
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        protected override float ComputeUtility()
        {
            m_childFactor?.UpdateUtility();
            return Evaluate(m_childFactor?.Utility ?? 0f);
        }

        protected abstract float Evaluate(float childUtility);

        #endregion
    }
}

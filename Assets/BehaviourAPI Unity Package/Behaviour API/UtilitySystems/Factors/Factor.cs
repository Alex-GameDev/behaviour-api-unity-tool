using BehaviourAPI.Core;
using System;

namespace BehaviourAPI.UtilitySystems
{
    public abstract class Factor : UtilityNode
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxInputConnections => -1;
        public override Type ChildType => typeof(Factor);

        #endregion

        protected override float GetUtility()
        {
            return MathUtilities.Clamp01(ComputeUtility());
        }

        protected abstract float ComputeUtility();
    }
}

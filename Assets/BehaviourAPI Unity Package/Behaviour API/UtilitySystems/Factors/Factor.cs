using BehaviourAPI.Core;
using System;

namespace BehaviourAPI.UtilitySystems
{
    /// <summary>
    /// Node that computes its utility clamped between 0 and 1.
    /// </summary>
    public abstract class Factor : UtilityNode
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxInputConnections => -1;
        public override Type ChildType => typeof(Factor);

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// Clamp the utility computed.
        /// </summary>
        /// <returns>The utility clamped between 0 and 1.</returns>
        protected override float GetUtility()
        {
            return MathUtilities.Clamp01(ComputeUtility());
        }

        /// <summary>
        /// Method used to calculate the utility.
        /// </summary>
        /// <returns>The utility of the factor.</returns>
        protected abstract float ComputeUtility();
    }
}

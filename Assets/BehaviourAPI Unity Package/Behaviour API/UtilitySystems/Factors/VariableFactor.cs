using System;

namespace BehaviourAPI.UtilitySystems
{
    public class VariableFactor : Factor
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxOutputConnections => 0;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public Func<float> Variable;

        public float min = 0f, max = 1f;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        protected override float ComputeUtility()
        {
            Utility = Variable?.Invoke() ?? min;
            Utility = (Utility - min) / (max - min);
            return Utility;
        }

        #endregion
    }
}

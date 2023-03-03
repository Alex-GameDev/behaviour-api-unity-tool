namespace BehaviourAPI.UtilitySystems.Factors
{
    public class ConstantFactor : Factor
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxOutputConnections => 0;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public float value;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        protected override float ComputeUtility()
        {
            return value;
        }

        #endregion
    }
}

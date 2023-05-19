namespace BehaviourAPI.UtilitySystems
{
    /// <summary>
    /// Leaf factor which have a constant utility value
    /// </summary>
    public class ConstantFactor : Factor
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxOutputConnections => 0;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        /// <summary>
        /// The utility value.
        /// </summary>
        public float value;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// Returns the constant value.
        /// </summary>
        /// <returns><see cref="value"/></returns>
        protected override float ComputeUtility()
        {
            return value;
        }

        #endregion
    }
}

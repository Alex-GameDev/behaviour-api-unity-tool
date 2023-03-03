using System;

namespace BehaviourAPI.UtilitySystems
{
    using Core;

    public abstract class UtilityNode : Node, IUtilityHandler
    {

        #region ------------------------------------------ Properties -----------------------------------------

        public float Utility 
        { 
            get => _utility; 
            protected set
            {
                if(_utility != value)
                {
                    _utility = value;
                    UtilityChanged?.Invoke(_utility);
                }
            }
        }

        float _utility;

        public Action<float> UtilityChanged { get; set; }

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public override object Clone()
        {
            UtilityNode node = (UtilityNode)base.Clone();
            node.UtilityChanged = delegate { };
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// Updates the current value of <see cref="Utility"/>
        /// </summary>
        public void UpdateUtility()
        {
            Utility = GetUtility();
        }

        protected abstract float GetUtility();

        #endregion
    }
}
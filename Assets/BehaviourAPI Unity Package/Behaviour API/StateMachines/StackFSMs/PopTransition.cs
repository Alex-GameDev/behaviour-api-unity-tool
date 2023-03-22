namespace BehaviourAPI.StateMachines.StackFSMs
{
    public class PopTransition : StackTransition
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxOutputConnections => 0;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override bool Perform()
        {
            bool canBePerformed = base.Perform();
            if (canBePerformed) _stackFSM.Pop(this);
            return canBePerformed;
        }

        #endregion
    }
}

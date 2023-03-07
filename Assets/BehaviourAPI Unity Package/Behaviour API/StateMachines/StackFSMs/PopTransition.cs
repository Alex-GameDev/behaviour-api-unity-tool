namespace BehaviourAPI.StateMachines.StackFSMs
{
    public class PopTransition : StackTransition
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxOutputConnections => 0;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Perform()
        {
            base.Perform();
            _stackFSM.Pop(this);
        }

        #endregion
    }
}

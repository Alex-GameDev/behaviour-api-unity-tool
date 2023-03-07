
namespace BehaviourAPI.UtilitySystems
{
    using Core;

    public class UtilityExitNode : UtilityExecutableNode
    {
        #region ------------------------------------------ Properties ----------------------------------------

        public Status ExitStatus;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override bool FinishExecutionWhenActionFinishes()
        {
            return true;
        }

        public override void Start()
        {
            if(ExitStatus != Status.None) Status = ExitStatus;
            else ExitStatus = Status.Running;
            BehaviourGraph.Finish(ExitStatus);
        }

        public override void Update()
        {
            return;
        }

        #endregion
    }
}

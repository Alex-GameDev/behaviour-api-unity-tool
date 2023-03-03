namespace BehaviourAPI.UtilitySystems
{
    using Core;
    using Action = Core.Actions.Action;

    public class UtilityAction : UtilityExecutableNode
    {
        #region ------------------------------------------ Properties ----------------------------------------
        
        public Action Action { get; set; }

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public bool FinishSystemOnComplete = false;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public override object Clone()
        {
            UtilityAction action = (UtilityAction)base.Clone();
            action.Action = (Action) Action?.Clone();
            return action;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            Status = Status.Running;
            Action?.Start();
        }

        public override void Update()
        {
            Status = Action?.Update() ?? Status.Running;
        }

        public override void Stop()
        {
            Status = Status.None;
            Action?.Stop();
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            Action?.SetExecutionContext(context);
        }

        public override bool FinishExecutionWhenActionFinishes() => FinishSystemOnComplete;

        #endregion
    }
}

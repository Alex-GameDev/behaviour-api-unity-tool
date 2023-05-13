namespace BehaviourAPI.UtilitySystems
{
    using Core;
    using Action = Core.Actions.Action;

    /// <summary>
    /// Utility node that executes an <see cref="Action"/> while selected.
    /// </summary>
    public class UtilityAction : UtilityExecutableNode
    {
        #region ------------------------------------------- Fields -------------------------------------------

        /// <summary>
        /// If true, when the <see cref="Action"/> end its execution with <see cref="Status.Success"/> or <see cref="Status.Failure"/>,
        /// end the <see cref="UtilitySystem"/> execution with this <see cref="Status"/> value.
        /// </summary>
        public bool FinishSystemOnComplete = false;

        /// <summary>
        /// The <see cref="Action"/> that this <see cref="UtilityAction"/> executes when is selected.
        /// </summary>
        public Action Action;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        /// <summary>
        /// <inheritdoc/>
        /// Clone the <see cref="Action"/> too.
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override object Clone()
        {
            UtilityAction action = (UtilityAction)base.Clone();
            action.Action = (Action)Action?.Clone();
            return action;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// <inheritdoc/>
        /// Start the <see cref="Action"/> execution.
        /// </summary>
        public override void Start()
        {
            base.Start();
            Action?.Start();
        }

        /// <summary>
        /// <inheritdoc/>.
        /// Is called when the <see cref="UtilityAction"/> is selected and start the <see cref="Action"/> too.
        /// </summary>
        public override void Update()
        {
            if (Status != Status.Running) return;

            Status = Action?.Update() ?? Status.Running;

            if (FinishSystemOnComplete && Status != Status.Running)
            {
                BehaviourGraph.Finish(Status);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// Is called when the utility action is no longer selected and Stop the action too.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            Action?.Stop();
        }



        /// <summary>
        /// <inheritdoc/>
        /// Passes the context to <see cref="Action"/>.
        /// </summary>
        /// <param name="context"><inheritdoc/></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            Action?.SetExecutionContext(context);
        }

        public override void Pause()
        {
            Action?.Pause();
        }

        public override void Unpause()
        {
            Action?.Unpause();
        }

        #endregion
    }
}

using System;

namespace BehaviourAPI.UtilitySystems
{
    using Core;

    /// <summary>
    /// Utility node that can be selected and executed by a utility system.
    /// </summary>
    public abstract class UtilitySelectableNode : UtilityNode, IStatusHandler
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxInputConnections => 1;

        /// <summary>
        /// The execution status of the node.
        /// </summary>
        public Status Status
        {
            get => _status;
            protected set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged?.Invoke(_status);
                }
            }
        }

        /// <summary>
        /// Event called when current status changed.
        /// </summary>
        public Action<Status> StatusChanged { get; set; }


        /// <summary>
        /// True if this element should be executed even if later elements have more utility:
        /// </summary>
        public bool ExecutionPriority { get; protected set; }

        #endregion

        #region -------------------------------------- Private variables -------------------------------------

        Status _status;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public override object Clone()
        {
            UtilitySelectableNode node = (UtilitySelectableNode)base.Clone();
            node.StatusChanged = (Action<Status>)StatusChanged?.Clone();
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// Is called when the <see cref="UtilitySelectableNode"/> is selected. 
        /// </summary>
        /// <exception cref="ExecutionStatusException">If it's already running.</exception>
        public virtual void Start()
        {
            if (Status != Status.None)
                throw new ExecutionStatusException(this, "ERROR: This node is already been executed");

            Status = Status.Running;
        }

        /// <summary>
        /// Is called each frame the <see cref="UtilitySelectableNode"/> is selected.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Is called when the <see cref="UtilitySelectableNode"/> is no longer selected or the <see cref="UtilitySystem"/> was stopped.
        /// </summary>
        /// <exception cref="ExecutionStatusException">If it's not running.</exception>
        public virtual void Stop()
        {
            if (Status == Status.None)
                throw new ExecutionStatusException(this, "ERROR: This node is already been stopped");

            Status = Status.None;
        }

        public abstract void Pause();

        public abstract void Unpause();

        #endregion
    }
}
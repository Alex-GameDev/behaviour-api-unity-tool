using System;

namespace BehaviourAPI.UtilitySystems
{
    using Core;

    /// <summary>
    /// Utility node that implements IStatusHandler
    /// </summary>
    public abstract class UtilitySelectableNode : UtilityNode, IStatusHandler
    {
        #region ------------------------------------------ Properties -----------------------------------------

        public override int MaxInputConnections => 1;

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

        public Status LastExecutionStatus => _lastExecutionStatus;

        public Action<Status> StatusChanged { get; set; }

        Status _status;
        Status _lastExecutionStatus;

        /// <summary>
        /// True if this element should be executed even if later elements have more utility:
        /// </summary>
        public bool ExecutionPriority { get; protected set; }

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public override object Clone()
        {
            UtilitySelectableNode node = (UtilitySelectableNode)base.Clone();
            node.StatusChanged = delegate { };
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public virtual void Start()
        {
            if (Status != Status.None)
                throw new Exception("ERROR: This node is already been executed");

            Status = Status.Running;
        }

        public abstract void Update();

        public virtual void Stop()
        {
            if (Status == Status.None)
                throw new Exception("ERROR: This node is already been stopped");

            _lastExecutionStatus = Status;
            Status = Status.None;
        }

        /// <summary>
        /// Return true if the utility system should change its status when a selectable node finish its execution
        /// </summary>
        /// <returns></returns>
        public abstract bool FinishExecutionWhenActionFinishes(); 
        #endregion
    }
}
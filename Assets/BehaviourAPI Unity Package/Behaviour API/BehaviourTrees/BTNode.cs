using System;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// The base node in the <see cref="BehaviourTree"/>.
    /// </summary>
    public abstract class BTNode : Node, IStatusHandler
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public override int MaxInputConnections => 1;
        public override Type ChildType => typeof(BTNode);

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

        #endregion

        public override object Clone()
        {
            var btNode = (BTNode)base.Clone();
            btNode.StatusChanged = delegate { };
            return btNode;
        }

        #region --------------------------------------- Runtime methods --------------------------------------

        public virtual void Start()
        {
            if (Status != Status.None)
                throw new Exception("ERROR: This node is already been executed");

            Status = Status.Running;
        }

        public void Update()
        {
            if (Status != Status.Running) return;
            Status = UpdateStatus();
        }

        public virtual void Stop()
        {
            if (Status == Status.None)
                throw new Exception("ERROR: This node is already been stopped");

            _lastExecutionStatus = Status;
            Status = Status.None;
        }

        protected abstract Status UpdateStatus();

        #endregion
    }
}
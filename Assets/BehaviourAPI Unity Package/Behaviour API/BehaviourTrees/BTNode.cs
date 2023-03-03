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

        public Action<Status> StatusChanged { get; set; }

        Status _status;

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
            Status = Status.Running;
        }

        public void Update()
        {
            Status = UpdateStatus();
        }

        public virtual void Stop()
        {
            Status = Status.None;
        }

        protected abstract Status UpdateStatus();

        #endregion
    }
}
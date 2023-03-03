namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// Node that execute its child node until returns a given value.
    /// </summary>
    public class LoopUntilNode : DirectDecoratorNode
    {
        #region ----------------------------------------- Properties -----------------------------------------

        int _currentIterations;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public Status TargetStatus = Status.Success;

        public int MaxIterations = -1;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public LoopUntilNode SetTargetStatus(Status status)
        {
            TargetStatus = status;
            return this;
        }

        public LoopUntilNode SetMaxIterations(int maxIterations)
        {
            MaxIterations = maxIterations;
            return this;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            base.Start();
            _currentIterations = 0;
        }

        protected override Status ModifyStatus(Status childStatus)
        {
            // If child execution ends without the target value, restart until currentIterations == MaxIterations
            if (childStatus == TargetStatus.Inverted())
            {
                _currentIterations++;
                if (_currentIterations != MaxIterations)
                {
                    // Restart the node execution
                    childStatus = Status.Running;
                    m_childNode.Stop();
                    m_childNode.Start();
                }
            }
            return childStatus;
        }

        #endregion
    }
}
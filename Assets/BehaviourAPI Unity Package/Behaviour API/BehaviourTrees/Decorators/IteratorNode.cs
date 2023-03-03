namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// Node that execute its child node the number of times determined by <see cref="Itera"/>
    /// </summary>
    public  class IteratorNode : DirectDecoratorNode
    {
        #region ----------------------------------------- Properties -----------------------------------------

        int _currentIterations;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public int Iterations = -1;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public IteratorNode SetIterations(int iterations)
        {
            Iterations = iterations;  
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
            // If child execution ends, restart until currentIterations > Iterations
            if (childStatus != Status.Running)
            {
                _currentIterations++;
                if (Iterations == -1 || _currentIterations < Iterations)
                {
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

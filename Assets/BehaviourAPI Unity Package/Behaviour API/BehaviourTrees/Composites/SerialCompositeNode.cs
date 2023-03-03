namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// Composite node that executes its children sequencially.
    /// </summary>
    public abstract class SerialCompositeNode : CompositeNode
    {
        #region ------------------------------------------ Properties -----------------------------------------

        int currentChildIdx = 0;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            currentChildIdx = 0;
            base.Start();
            GetCurrentChild().Start();
        }

        public override void Stop()
        {
            base.Stop();
            GetCurrentChild().Stop();
        }
        protected override Status UpdateStatus()
        {
            BTNode currentChild = GetCurrentChild();
            currentChild.Update();
            var status = currentChild.Status;
            if (KeepExecuting(status))
            {
                if (TryGoToNextChild())
                {
                    status = Status.Running;
                }
                else
                {
                    status = GetFinalStatus(status);
                }
            }
            else
            {
                status = GetFinalStatus(status);
            }

            return status;
        }

        protected abstract bool KeepExecuting(Status status);
        protected abstract Status GetFinalStatus(Status status);

        private bool TryGoToNextChild()
        {
            if (currentChildIdx < ChildCount - 1)
            {
                GetCurrentChild().Stop();
                currentChildIdx++;
                GetCurrentChild().Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        private BTNode GetCurrentChild() => GetBTChildAt(currentChildIdx);

        #endregion
    }
}
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

        /// <summary>
        /// <inheritdoc/>
        /// Starts the first node execution.
        /// </summary>
        public override void Start()
        {
            currentChildIdx = 0;
            base.Start();
            GetCurrentChild().Start();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Stop the current executed child.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            GetCurrentChild().Stop();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Update the execution of the current child. If it returned status is not the final
        /// status, stop the child and starts the next. If there are no more childs, return the final status.
        /// </summary>
        /// <returns>Running if no child has returned the target status and all childs were not executed yet. Else returns the status of the last node.</returns>
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

        /// <summary>
        /// Return if <paramref name="status"/> is not the target value.
        /// </summary>
        /// <param name="status">The current status of the child.</param>
        /// <returns>true if <paramref name="status"/> is not the target value, false otherwise. </returns>
        protected abstract bool KeepExecuting(Status status);

        /// <summary>
        /// Get the final status of the composite node (must be success or failure).
        /// </summary>
        /// <param name="status">The current status of the node.</param>
        /// <returns>The final execution status of the node.</returns>
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
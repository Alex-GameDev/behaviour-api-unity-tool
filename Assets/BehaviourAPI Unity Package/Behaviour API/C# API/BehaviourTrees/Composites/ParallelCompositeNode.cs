using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// Composite node that executes all its children in all execution frames, until one of them returns the trigger value.
    /// </summary>
    public class ParallelCompositeNode : CompositeNode
    {
        #region ------------------------------------------- Fields -------------------------------------------

        /// <summary>
        /// The status value that any of the children must reach to end the execution of all nodes.
        /// </summary>
        public Status TriggerStatus = Status.Failure;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// <inheritdoc/>
        /// Starts all its children.
        /// </summary>
        public override void OnStarted()
        {
            base.OnStarted();
            m_children.ForEach(c => c?.OnStarted());
        }

        /// <summary>
        /// <inheritdoc/>
        /// Stop all its children.
        /// </summary>
        public override void OnStopped()
        {
            base.OnStopped();

            m_children.ForEach(c => c?.OnStopped());
        }

        /// <summary>
        /// <inheritdoc/>
        /// Pauses all its children.
        /// </summary>
        public override void OnPaused()
        {
            m_children.ForEach(c => c?.OnPaused());
        }

        /// <summary>
        /// <inheritdoc/>
        /// Unpauses all its children.
        /// </summary>
        public override void OnUnpaused()
        {
            m_children.ForEach(c => c?.OnUnpaused());
        }

        /// <summary>
        /// <inheritdoc/>
        /// Update all its children node. The returned <see cref="Status"/> value depends on the children status and the value in <see cref="TriggerStatus"/>.
        /// </summary>
        /// <returns><see cref="TriggerStatus"/> if any of the nodes end with <see cref="TriggerStatus"/>, else if all of the nodes end, return the oposite status, else return running.</returns>
        /// <exception cref="MissingChildException">If the child list is empty.</exception>
        protected override Status UpdateStatus()
        {
            if (m_children.Count == 0) throw new MissingChildException(this, "This composite has no childs");

            m_children.ForEach(c => c.OnUpdated());
            List<Status> allStatus = m_children.Select(c => c.Status).ToList();

            // Check for trigger value
            if (allStatus.Contains(TriggerStatus)) return TriggerStatus;

            // Check if execution finish
            if (!allStatus.Contains(Status.Running)) return TriggerStatus == Status.Success ? Status.Failure : Status.Success;

            return Status.Running;
        }


        #endregion
    }
}

using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.BehaviourTrees
{
    using Core.Exceptions;
    using Core;

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
        public override void Start()
        {
            base.Start();
            m_children.ForEach(c => c?.Start());
        }

        /// <summary>
        /// <inheritdoc/>
        /// Stop all its children.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            m_children.ForEach(c => c?.Stop());
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

            m_children.ForEach(c => c.Update());
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

using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.BehaviourTrees
{
    using Core.Exceptions;
    using Core;

    /// <summary>
    /// Composite node that executes its children at the same time, until one of them returns the trigger value.
    /// </summary>
    public class ParallelCompositeNode : CompositeNode
    {
        #region ------------------------------------------- Fields -------------------------------------------

        public Status TriggerStatus = Status.Failure;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            base.Start();
            m_children.ForEach(c => c?.Start());
        }

        public override void Stop()
        {
            base.Stop();

            m_children.ForEach(c => c?.Stop());
        }

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

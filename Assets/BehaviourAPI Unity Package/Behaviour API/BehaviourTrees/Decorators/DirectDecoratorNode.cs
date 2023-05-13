namespace BehaviourAPI.BehaviourTrees
{
    using BehaviourAPI.Core;

    /// <summary>
    /// Decorator that always execute its child.
    /// </summary>
    public abstract class DirectDecoratorNode : DecoratorNode
    {
        /// <summary>
        /// <inheritdoc/>
        /// Starts the execution of its child.
        /// </summary>
        /// <exception cref="MissingChildException">If child is null.</exception>
        public override void Start()
        {
            base.Start();

            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Start();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Stops the execution of its child.
        /// </summary>
        /// <exception cref="MissingChildException">If child is null.</exception>
        public override void Stop()
        {
            base.Stop();

            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Stop();
        }

        public override void Pause()
        {
            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Pause();
        }

        public override void Unpause()
        {
            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Unpause();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Updates the execution of its child and returns the value modified.
        /// </summary>
        /// <exception cref="MissingChildException">If child is null.</exception>
        protected override Status UpdateStatus()
        {
            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Update();
            var status = m_childNode.Status;
            return ModifyStatus(status);
        }

        /// <summary>
        /// Gets the children status and return it modified.
        /// </summary>
        /// <param name="childStatus">The child current status.</param>
        /// <returns>The child status modified.</returns>
        protected abstract Status ModifyStatus(Status childStatus);
    }
}

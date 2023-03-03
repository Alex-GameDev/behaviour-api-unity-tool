namespace BehaviourAPI.BehaviourTrees
{
    using BehaviourAPI.Core;
    using Core.Exceptions;

    /// <summary>
    /// Decorator that always execute its child.
    /// </summary>
    public abstract class DirectDecoratorNode : DecoratorNode
    {
        public override void Start()
        {
            base.Start();

            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Stop();
        }

        protected override Status UpdateStatus()
        {
            if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

            m_childNode.Update();
            var status = m_childNode.Status;
            return ModifyStatus(status);
        }

        protected abstract Status ModifyStatus(Status childStatus);
    }
}

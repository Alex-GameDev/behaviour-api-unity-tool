using System;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;
    using Core.Perceptions;

    /// <summary>
    /// Decorator that executes its child only if a perception is triggered. Perception is checked at the start
    /// and return Failure if isn't triggered. Otherwise execute the child and returns its value.
    /// </summary>
    public class ConditionNode : DecoratorNode
    {
        #region ------------------------------------------ Properties -----------------------------------------

        bool _executeChild;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        public Perception Perception;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public ConditionNode SetPerception(Perception perception)
        {
            Perception = perception;
            return this;
        }

        public override object Clone()
        {
            var node = (ConditionNode)base.Clone();
            node.Perception = (Perception)Perception?.Clone();
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            base.Start();
            if (Perception != null)
            {
                Perception.Initialize();
                _executeChild = Perception.Check();
                Perception.Reset();                
            }
            else
                throw new NullReferenceException("ERROR: Perception is not defined.");

            if (_executeChild)
            {
                m_childNode.Start();
            }
        }

        public override void Stop()
        {
            base.Stop();
            if(_executeChild)
            {
                m_childNode.Stop();
            }            
        }

        protected override Status UpdateStatus()
        {
            if (_executeChild)
            {
                if (m_childNode == null) throw new MissingChildException(this, "This decorator has no child");

                m_childNode.Update();
                return m_childNode.Status;
            }
            else
            {                    
                return Status.Failure;
            }
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            Perception?.SetExecutionContext(context);
        }

        #endregion
    }
}

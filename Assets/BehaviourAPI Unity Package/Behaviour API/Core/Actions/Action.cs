using System;

namespace BehaviourAPI.Core.Actions
{
    public abstract class Action : ICloneable
    {
        public abstract void Start();
        public abstract Status Update();
        public abstract void Stop();

        public abstract void SetExecutionContext(ExecutionContext context);

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}

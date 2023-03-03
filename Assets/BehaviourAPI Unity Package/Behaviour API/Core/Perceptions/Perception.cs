using System;

namespace BehaviourAPI.Core.Perceptions
{
    public abstract class Perception : ICloneable
    {
        public virtual void Initialize() { }
        public abstract bool Check();
        public virtual void Reset() { }

        public abstract void SetExecutionContext(ExecutionContext context);

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}

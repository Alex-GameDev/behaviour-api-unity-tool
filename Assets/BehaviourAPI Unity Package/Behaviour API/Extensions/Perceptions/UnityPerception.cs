namespace BehaviourAPI.UnityExtensions
{
    using Core;
    using Core.Perceptions;

    public abstract class UnityPerception : Perception
    {
        protected UnityExecutionContext context;
        public virtual string DisplayInfo => "Unity Perception";
        public Perception Build() => new ConditionPerception(Initialize, Check, Reset);

        public override void SetExecutionContext(ExecutionContext context)
        {
            this.context = (UnityExecutionContext)context;
            OnSetContext();
        }

        protected virtual void OnSetContext()
        {
        }
    }
}

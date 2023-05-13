using System;

namespace BehaviourAPI.Core.Perceptions
{
    /// <summary>
    /// Represents a perception created with custom methods.
    /// </summary>
    public class ConditionPerception : Perception
    {
        public Func<bool> onCheck;
        public Action onInit;
        public Action onReset;
        public Action onPause;
        public Action onUnpause;

        /// <summary>
        /// Create a <see cref="ConditionPerception"/> that execute a delegate on Init, Check and reset.
        /// </summary>
        /// <param name="onInit">The delegate executed in <see cref="Initialize"/> event. </param>
        /// <param name="onCheck">The function executed in <see cref="Check"/> event. </param>
        /// <param name="onReset">The delegate executed in <see cref="Reset"/> event. </param>
        public ConditionPerception(Action onInit, Func<bool> onCheck, Action onReset = null)
        {
            this.onInit = onInit;
            this.onCheck = onCheck;
            this.onReset = onReset;
        }

        /// <summary>
        /// Create a <see cref="ConditionPerception"/> that execute a delegate on Check and, optionally on reset.
        /// </summary>
        /// <param name="onCheck">The function executed in <see cref="Check"/> event. </param>
        /// <param name="onReset">The delegate executed in <see cref="Reset"/> event. </param>
        public ConditionPerception(Func<bool> onCheck, Action onReset = null)
        {
            this.onCheck = onCheck;
            this.onReset = onReset;
        }

        public ConditionPerception()
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the init delegate.
        /// </summary>
        public override void Initialize() => onInit?.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the check function and returns its returned value.
        /// </summary>
        public override bool Check() => onCheck.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the reset delegate.
        /// </summary>
        public override void Reset() => onReset?.Invoke();

        public override void Pause() => onPause?.Invoke();

        public override void Unpause() => onUnpause?.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// (The context in Condition perceptions is not used).
        /// </summary>
        public override void SetExecutionContext(ExecutionContext context)
        {
            return;
        }

    }
}

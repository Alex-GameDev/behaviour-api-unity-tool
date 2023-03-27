using System;

namespace BehaviourAPI.Core.Perceptions
{
    /// <summary>
    /// Represents a perception created with custom methods.
    /// </summary>
    public class ConditionPerception : Perception
    {
        Func<bool> _onCheck;
        Action _onInit;
        Action _onReset;

        /// <summary>
        /// Create a <see cref="ConditionPerception"/> that execute a delegate on Init, Check and reset.
        /// </summary>
        /// <param name="onInit">The delegate executed in <see cref="Initialize"/> event. </param>
        /// <param name="onCheck">The function executed in <see cref="Check"/> event. </param>
        /// <param name="onReset">The delegate executed in <see cref="Reset"/> event. </param>
        public ConditionPerception(Action onInit, Func<bool> onCheck, Action onReset = null)
        {
            _onInit = onInit;
            _onCheck = onCheck;
            _onReset = onReset;
        }

        /// <summary>
        /// Create a <see cref="ConditionPerception"/> that execute a delegate on Check and, optionally on reset.
        /// </summary>
        /// <param name="onCheck">The function executed in <see cref="Check"/> event. </param>
        /// <param name="onReset">The delegate executed in <see cref="Reset"/> event. </param>
        public ConditionPerception(Func<bool> check, Action stop = null)
        {
            _onCheck = check;
            _onReset = stop;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the init delegate.
        /// </summary>
        public override void Initialize() => _onInit?.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the check function and returns its returned value.
        /// </summary>
        public override bool Check() => _onCheck.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the reset delegate.
        /// </summary>
        public override void Reset() => _onReset?.Invoke();

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

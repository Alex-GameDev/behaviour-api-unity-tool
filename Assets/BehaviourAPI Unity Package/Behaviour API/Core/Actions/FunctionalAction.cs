 using System;

namespace BehaviourAPI.Core.Actions
{
    /// <summary>
    /// Represents an action created with custom methods.
    /// </summary>
    public class FunctionalAction : Action
    {
        Func<Status> _update;

        System.Action _start;
        System.Action _stop;

        /// <summary>
        /// Create a <see cref="FunctionalAction"/> that executes a delegate on Start, Update and stop.
        /// </summary>
        /// <param name="start">The delegate executed in <see cref="Start"/> event.</param>
        /// <param name="update">The function executed in <see cref="Update"/> event.</param>
        /// <param name="stop">The delegate executed in <see cref="Stop"/> event.</param>
        public FunctionalAction(System.Action start, Func<Status> update, System.Action stop = null)
        {
            _start = start;
            _update = update;
            _stop = stop;
        }

        /// <summary>
        /// Create a <see cref="FunctionalAction"/> that executes a delegate on Update and optionally, a method on stop.
        /// </summary>
        /// <param name="update">The function executed in <see cref="Update"/> event.</param>
        /// <param name="stop">The delegate executed in <see cref="Stop"/> event.</param>
        public FunctionalAction(Func<Status> update, System.Action stop = null)
        {
            _update = update;
            _stop = stop;
        }

        /// <summary>
        /// Create a <see cref="FunctionalAction"/> that executes a method when started and only returns <see cref="Status.Running"/> on Update.
        /// </summary>
        /// <param name="start">The delegate executed in <see cref="Start"/> event.</param>
        public FunctionalAction(System.Action start)
        {
            _start = start;
            _update = () => Status.Running;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the start delegate.
        /// </summary>
        public override void Start() => _start?.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the update function and returns its returned value.
        /// </summary>
        public override Status Update() => _update.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the stop delegate.
        /// </summary>
        public override void Stop() => _stop?.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// (The context in Functional actions is not used).
        /// </summary>
        public override void SetExecutionContext(ExecutionContext context)
        {
            return;
        }
    }
}

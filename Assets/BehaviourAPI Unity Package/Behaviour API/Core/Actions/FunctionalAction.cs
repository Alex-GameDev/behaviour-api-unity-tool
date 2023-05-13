using System;

namespace BehaviourAPI.Core.Actions
{
    /// <summary>
    /// Represents an action created with custom methods.
    /// </summary>
    public class FunctionalAction : Action
    {
        public Func<Status> onUpdate;

        public System.Action onStart;
        public System.Action onStop;
        public System.Action onPause;
        public System.Action onUnpause;

        /// <summary>
        /// Create a <see cref="FunctionalAction"/> that executes a delegate on Start, Update and stop.
        /// </summary>
        /// <param name="start">The delegate executed in <see cref="Start"/> event.</param>
        /// <param name="update">The function executed in <see cref="Update"/> event.</param>
        /// <param name="stop">The delegate executed in <see cref="Stop"/> event.</param>
        public FunctionalAction(System.Action start, Func<Status> update, System.Action stop = null)
        {
            onStart = start;
            onUpdate = update;
            onStop = stop;
        }

        /// <summary>
        /// Create a <see cref="FunctionalAction"/> that executes a delegate on Update and optionally, a method on stop.
        /// </summary>
        /// <param name="update">The function executed in <see cref="Update"/> event.</param>
        /// <param name="stop">The delegate executed in <see cref="Stop"/> event.</param>
        public FunctionalAction(Func<Status> update, System.Action stop = null)
        {
            onUpdate = update;
            onStop = stop;
        }

        /// <summary>
        /// Create a <see cref="FunctionalAction"/> that executes a method when started and only returns <see cref="Status.Running"/> on Update.
        /// </summary>
        /// <param name="start">The delegate executed in <see cref="Start"/> event.</param>
        public FunctionalAction(System.Action start)
        {
            onStart = start;
            onUpdate = () => Status.Running;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the start delegate.
        /// </summary>
        public override void Start() => onStart?.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the update function and returns its returned value.
        /// </summary>
        public override Status Update() => onUpdate.Invoke();

        /// <summary>
        /// <inheritdoc/>
        /// Invoke the stop delegate.
        /// </summary>
        public override void Stop() => onStop?.Invoke();

        public override void Pause() => onPause?.Invoke();

        public override void Unpause() => onUnpause?.Invoke();

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

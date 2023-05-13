using System;

namespace BehaviourAPI.Core
{
    /// <summary>
    /// Basic class for all behaviour systems. 
    /// </summary>
    public abstract class BehaviourEngine : IStatusHandler
    {
        #region ----------------------------------------- Properties -------------------------------------------

        public Status Status
        {
            get => _status;
            protected set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged?.Invoke(_status);
                }
            }
        }

        public Action<Status> StatusChanged { get; set; }

        /// <summary>
        /// Gets if the behaviour is currently paused.
        /// </summary>
        /// <value>True if its paused, false otherwise.</value>
        public bool IsPaused { get; private set; }

        #endregion

        #region -------------------------------------- Private variables -------------------------------------

        Status _status;

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// Call this method at the beginning of the execution (before calling Update) 
        /// to set <see cref="Status"/> to Running. Use only if its not a subsystem.
        /// </summary>
        /// <exception cref="ExecutionStatusException">If the graph is already in execution.</exception>
        public virtual void Start()
        {
            if (Status != Status.None)
                throw new ExecutionStatusException(this, "ERROR: This behaviour engine is already been executed");

            Status = Status.Running;
        }

        /// <summary>
        /// Call this method at the end of the execution to restart it <see cref="Status"/>.
        /// </summary>
        /// <exception cref="ExecutionStatusException">If the graph is not in execution.</exception>
        public virtual void Stop()
        {
            if (Status == Status.None)
                throw new ExecutionStatusException(this, "ERROR: This behaviour engine is already been stopped");

            Status = Status.None;
        }

        /// <summary>
        /// Called every execution frame.
        /// </summary>
        /// <exception cref="ExecutionStatusException">Throws if attempt to update when its paused.</exception>
        public void Update()
        {
            if (IsPaused)
                throw new ExecutionStatusException(this, "Behaviour engine cannot be updated if is paused");

            if (Status != Status.Running) return; // Graph already finished
            Execute();
        }

        /// <summary>
        /// Call this method when its executing to pauses the execution temporally. When the behaviour is 
        /// paused it can't be updated. Unpause the behaviour execution using the <see cref="Unpause"/> method.
        /// </summary>
        /// <exception cref="ExecutionStatusException">If the graph is not in execution.</exception>
        public virtual void Pause()
        {
            if (IsPaused)
                throw new ExecutionStatusException(this, "ERROR: Trying to pause a behaviour engine that is already been paused");

            IsPaused = true;
        }

        /// <summary>
        /// Call this method when its executing to pauses the execution temporally. When the behaviour is 
        /// paused it can't be updated. Unpause the behaviour execution using the <see cref="Unpause"/> method.
        /// </summary>
        /// <exception cref="ExecutionStatusException">If the graph is not in execution.</exception>
        public virtual void Unpause()
        {
            if (!IsPaused)
                throw new ExecutionStatusException(this, "ERROR: Trying to unpause a behaviour engine that is not paused.");

            IsPaused = true;
        }

        /// <summary>
        /// Finish the graph execution with the status given. 
        /// </summary>
        /// <param name="executionResult">The final value for <see cref="Status"></see>. Must be <see cref="Status.Success"/> or <see cref="Status.Failure"/>. </param>
        /// <exception cref="ExecutionStatusException">If the value passed as argument is not success or failure.</exception>
        public void Finish(Status executionResult)
        {
            if (executionResult == Status.Running || executionResult == Status.None)
                throw new ExecutionStatusException(this, $"Error: BehaviourEngine execution result can't be {executionResult}");

            Status = executionResult;
        }

        /// <summary>
        /// Set the execution context of the system.
        /// </summary>
        /// <param name="context">The <see cref="ExecutionContext"/> used.</param>
        public abstract void SetExecutionContext(ExecutionContext context);

        /// <summary>
        /// Stop the execution and start it right after.
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Executes every frame if the execution has not finished yet. 
        /// </summary>
        protected abstract void Execute();

        #endregion
    }
}

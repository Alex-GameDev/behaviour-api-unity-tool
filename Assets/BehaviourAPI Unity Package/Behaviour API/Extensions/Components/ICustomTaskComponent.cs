namespace BehaviourAPI.UnityExtensions
{
    using Core;
    /// <summary>
    /// Component that allows changing the actions for each agent with the same behavior system.
    /// to customize
    /// </summary>
    public interface ICustomTaskComponent
    {
        /// <summary>
        /// Create a new task ready to be performed.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <returns>A unique id for the task, or null if couldn't be created.</returns>
        string CreateTask(string name);

        /// <summary>
        /// Checks if the task that correspond to the given name can be performed.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <returns>True if the task can be performed, false otherwise.</returns>
        bool CanPerformTask(string name);

        /// <summary>
        /// Starts the task that corresponds to the id passed as a parameter.
        /// </summary>
        /// <param name="id">The id of the task that will be started.</param>
        /// <returns>True if the task exists and can be started.</returns>
        bool StartTask(string id);

        /// <summary>
        /// Get the current status of a task that is being executed.
        /// </summary>
        /// <param name="id">The id of the task.</param>
        /// <returns>The status of the task if exist and its being executed, None otherwise.</returns>
        Status GetTaskStatus(string id);

        /// <summary>
        /// Stops the task that corresponds to the id passed as a parameter.
        /// </summary>
        /// <param name="id">The id of the task that will be stopped.</param>
        /// <returns>True if the task exists and can be canceled.</returns>
        bool CancelTask(string id);

        /// <summary>
        /// Pauses the task that corresponds to the id passed as a parameter.
        /// </summary>
        /// <param name="id">The id of the task that will be paused.</param>
        /// <returns>True if the task exists and can be paused.</returns>
        bool PauseTask(string id);

        /// <summary>
        /// Resumes the task that corresponds to the id passed as a parameter.
        /// </summary>
        /// <param name="id">The id of the task that will be paused. </param>
        /// <returns>True if the task exists and can be unpaused.</returns>
        bool UnpauseTask(string id);
    }
}

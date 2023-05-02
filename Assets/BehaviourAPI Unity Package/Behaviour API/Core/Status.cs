namespace BehaviourAPI.Core
{
    /// <summary>
    /// Defines the execution status of an element.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The element is not being executed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The element is being executed.
        /// </summary>
        Running = 1,

        /// <summary>
        /// The element execution ended with success.
        /// </summary>
        Success = 2,

        /// <summary>
        /// The element execution ended with failure.
        /// </summary>
        Failure = 4
    }

    /// <summary>
    /// Flags used to define various Status values in the same variable.
    /// </summary>
    [System.Flags]
    public enum StatusFlags
    {
        /// <summary>
        /// Equivalent to Status.None
        /// </summary>
        None = 0,

        /// <summary>
        /// Equivalent to Status.Running
        /// </summary>
        Running = 1,

        /// <summary>
        /// Equivalent to Status.Success
        /// </summary>
        Success = 2,

        /// <summary>
        /// Equivalent to Status.Running | Status.Success
        /// </summary>
        NotFailure = 3,

        /// <summary>
        /// Equivalent to Status.Failure
        /// </summary>
        Failure = 4,

        /// <summary>
        /// Equivalent to Status.Running | Status.Failure
        /// </summary>
        NotSuccess = 5,

        /// <summary>
        /// Equivalent to Status.Success | Status.Failure
        /// </summary>
        Finished = 6,

        /// <summary>
        /// Equivalent to Status.Running | Status.Success | Status.Failure
        /// </summary>
        Active = 7
    }
}

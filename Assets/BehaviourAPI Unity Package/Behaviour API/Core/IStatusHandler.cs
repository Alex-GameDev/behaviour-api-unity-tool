using System;

namespace BehaviourAPI.Core
{
    public interface IStatusHandler
    {
        Action<Status> StatusChanged { get; set; }

        /// <summary>
        /// The current status of the element
        /// </summary>
        Status Status { get; }

        /// <summary>
        /// Called the first frame of the execution
        /// </summary>
        void Start();

        /// <summary>
        /// Called every execution frame
        /// </summary>
        void Update();

        /// <summary>
        /// Called when execution ends.
        /// </summary>
        void Stop();
    }

}
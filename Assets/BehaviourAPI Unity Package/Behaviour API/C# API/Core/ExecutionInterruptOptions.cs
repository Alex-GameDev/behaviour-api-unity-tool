using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Core
{
    /// <summary>
    /// Defines what to do when a element execution is interrupted (try to stop without finish first).
    /// </summary>
    public enum ExecutionInterruptOptions
    {
        /// <summary>
        /// Dont call any event
        /// </summary>
        None = 0,

        /// <summary>
        /// Call stop event
        /// </summary>
        Pause = 1,

        /// <summary>
        /// Call pause event
        /// </summary>
        Stop = 2
    }
}
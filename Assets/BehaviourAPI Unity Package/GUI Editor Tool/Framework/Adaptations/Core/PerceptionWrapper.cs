using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Perceptions;

    /// <summary>
    /// Adaptation wrapper class for serialize <see cref="ConditionPerception"/> subperception in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    [Serializable]
    public class PerceptionWrapper : Perception
    {
        /// <summary>
        /// The wrapped perception
        /// </summary>
        [SerializeReference] public Perception perception;

        /// <summary>
        /// <inheritdoc/>
        /// Call <see cref="perception"/> initialize event.
        /// </summary>
        public override void Initialize() => perception.Initialize();

        /// <summary>
        /// <inheritdoc/>
        /// Call <see cref="perception"/> check event.
        /// </summary>
        public override bool Check() => perception.Check();

        /// <summary>
        /// <inheritdoc/>
        /// Call <see cref="perception"/> reset event.
        /// </summary>
        public override void Reset() => perception.Reset();

        public override void Pause() => perception.Pause();

        public override void Unpause() => perception.Unpause();

        public override object Clone()
        {
            var copy = (PerceptionWrapper)base.Clone();
            copy.perception = (Perception)perception.Clone();
            return copy;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Passes the context to <see cref="perception"/>.
        /// </summary>
        /// <param name="context"></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            perception.SetExecutionContext(context);
        }


    }
}

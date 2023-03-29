using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Perceptions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="ConditionPerception"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class CompoundPerceptionWrapper : Perception, IBuildable
    {
        /// <summary>
        /// The wrapped compound perception.
        /// </summary>
        [SerializeReference] public CompoundPerception compoundPerception;

        /// <summary>
        /// The subperception serializable list.
        /// </summary>
        public List<PerceptionWrapper> subPerceptions = new List<PerceptionWrapper>();

        /// <summary>
        /// Parameterless constructor for reflection.
        /// </summary>
        public CompoundPerceptionWrapper()
        {
        }

        /// <summary>
        /// Create a new <see cref="CompoundPerceptionWrapper"></see> by a <see cref="CompoundPerception"/>. 
        /// </summary>
        /// <param name="compoundPerception"></param>
        public CompoundPerceptionWrapper(CompoundPerception compoundPerception)
        {
            this.compoundPerception = compoundPerception;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Call <see cref="compoundPerception"/> initialize event.
        /// </summary>
        public override void Initialize() => compoundPerception.Initialize();

        /// <summary>
        /// <inheritdoc/>
        /// Call <see cref="compoundPerception"/> check event.
        /// </summary>
        public override bool Check() => compoundPerception.Check();

        /// <summary>
        /// <inheritdoc/>
        /// Call <see cref="compoundPerception"/> reset event.
        /// </summary>
        public override void Reset() => compoundPerception.Reset();

        public override object Clone()
        {
            var copy = (CompoundPerceptionWrapper)base.Clone();
            copy.compoundPerception = (CompoundPerception)compoundPerception.Clone();
            copy.subPerceptions = subPerceptions.Select(p => (PerceptionWrapper)p.Clone()).ToList();
            return copy;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Passes the context to <see cref="compoundPerception"/>.
        /// </summary>
        /// <param name="context"></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            compoundPerception.SetExecutionContext(context);
        }

        /// <summary>
        /// <inheritdoc/>
        /// Set <see cref="compoundPerception"/> subperceptions from <see cref="subPerceptions"/> list.
        /// </summary>
        /// <param name="data"></param>
        public void Build(SystemData data)
        {
            compoundPerception.Perceptions = subPerceptions.Select(p => p.perception).ToList();
        }


    }
}

using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core.Actions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="SubsystemAction"/> in editor tools.
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class SubgraphAction : SubsystemAction, IBuildable
    {
        /// <summary>
        /// The guid of the subgraph in the system data.
        /// </summary>
        [GraphIdentificator] public string subgraphId;

        /// <summary>
        /// Reflection constructor.
        /// </summary>
        public SubgraphAction() : base(null)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// Set the <see cref="SubsystemAction.SubSystem"/> reference searching for a graph 
        /// with the id stored in <see cref="subgraphId"/>.
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        public void Build(SystemData data)
        {
            if (!string.IsNullOrEmpty(subgraphId))
            {
                var subgraphData = data.graphs.Find(g => g.id == subgraphId);
                if (subgraphData != null)
                {
                    SubSystem = subgraphData.graph;
                }
                else
                {
                    Debug.LogWarning("Build error: The subgraphId didn't match with any graph in the system");
                }
            }
        }
    }
}
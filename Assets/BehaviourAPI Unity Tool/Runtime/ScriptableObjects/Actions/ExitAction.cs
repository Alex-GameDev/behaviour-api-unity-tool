using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public class ExitAction : Action
    {
        [SerializeField] Status status;

        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
        }

        public override Status Update()
        {
            return Status.None;
        }
    }
}

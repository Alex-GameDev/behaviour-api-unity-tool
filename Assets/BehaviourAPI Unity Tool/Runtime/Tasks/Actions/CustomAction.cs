using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System;
using System.Linq.Expressions;
using UnityEngine;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomAction : Action
    {
        [SerializeField] SerializedAction start;
        [SerializeField] SerializedStatusFunction update;
        [SerializeField] SerializedAction stop;

        public override void Start()
        {
           
        }

        public override void Stop()
        {
           
        }

        public override Status Update()
        {
            return Status.Success;
        }
    }
}

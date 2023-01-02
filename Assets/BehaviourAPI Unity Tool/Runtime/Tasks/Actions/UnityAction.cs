using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class UnityAction : Action
    {
        public virtual string DisplayInfo => "Unity Action";
        public Status ExecutionStatus { get; private set; }

        public override void Start()
        {
            ExecutionStatus = Status.Running;
            OnStart();
        }

        public override void Stop()
        {
            ExecutionStatus = Status.None;
            OnStop();
        }

        public override Status Update()
        {
            OnUpdate();
            return ExecutionStatus;
        }

        protected void Success() => ExecutionStatus = Status.Success;
        protected void Failure() => ExecutionStatus = Status.Failure;

        protected virtual void OnStart() { }
        protected abstract void OnUpdate();
        protected virtual void OnStop() { }
    }
}

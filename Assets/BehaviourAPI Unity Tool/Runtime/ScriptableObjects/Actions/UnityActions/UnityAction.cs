using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class UnityAction : ActionAsset
    {
        public Status ExecutionStatus { get; private set; }
        public Action Build() => new FunctionalAction(
            () =>
            {
                ExecutionStatus = Status.Running;
                Start();
            }, 
            () =>
            {
                Update();
                return ExecutionStatus;
            }, 
            () =>
            {
                ExecutionStatus = Status.None;
                Stop();
            });

        protected virtual void Start() { }

        protected abstract void Update();

        protected virtual void Stop() { }
        protected void Success() => ExecutionStatus = Status.Success;
        protected void Failure() => ExecutionStatus = Status.Failure;

    }
}

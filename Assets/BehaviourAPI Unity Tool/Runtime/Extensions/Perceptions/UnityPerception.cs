using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public abstract class UnityPerception : Perception
    {
        protected UnityExecutionContext context;
        public virtual string DisplayInfo => "Unity Perception";
        public Perception Build() => new ConditionPerception(Initialize, Check, Reset);

        public override void SetExecutionContext(ExecutionContext context)
        {
            this.context = (UnityExecutionContext)context;
            OnSetContext();
        }

        protected virtual void OnSetContext()
        {
        }
    }
}

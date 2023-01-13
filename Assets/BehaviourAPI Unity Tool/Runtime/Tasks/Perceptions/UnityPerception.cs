using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class UnityPerception : Perception
    {
        public virtual string DisplayInfo => "Unity Perception";
        public Perception Build() => new ConditionPerception(Initialize, Check, Reset);
    }
}

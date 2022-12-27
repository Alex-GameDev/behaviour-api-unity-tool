using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomAction : Action
    {
        public Component component;
        public string MethodName;

        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        public override Status Update()
        {
            throw new System.NotImplementedException();
        }
    }
}

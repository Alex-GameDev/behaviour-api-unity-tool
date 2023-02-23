using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class DebugAction : UnityAction
    {
        public string message;

        public override string DisplayInfo => "Debug Log $message";

        public DebugAction()
        {
        }

        public DebugAction(string message)
        {
            this.message = message;
        }

        public override void Start()
        {
        }

        public override Status Update()
        {
            Debug.Log(message, context.GameObject);
            return Status.Success;
        }

        public override void Stop()
        {
        }


    }
}

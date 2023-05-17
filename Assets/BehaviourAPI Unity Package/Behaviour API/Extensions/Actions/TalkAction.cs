using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class TalkAction : UnityAction
    {
        public string text;

        public override string DisplayInfo => base.DisplayInfo;

        public override void Start()
        {
            context.Talk.StartTalk(text);
        }

        public override void Stop()
        {
            context.Talk.CancelTalk();
        }

        public override Status Update()
        {
            if (context.Talk.IsTalking())
            {
                return Status.Running;
            }
            else
            {
                return Status.Success;
            }
        }
    }
}

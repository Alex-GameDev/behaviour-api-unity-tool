using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class TalkAction : UnityAction
    {
        public string text;

        public float delay;

        float _currentDelay;

        public TalkAction()
        {
        }

        public TalkAction(string text, float delay)
        {
            this.text = text;
            this.delay = delay;
        }

        public override string DisplayInfo => base.DisplayInfo;

        public override object Clone()
        {
            return base.Clone();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Start()
        {
            context.Talk.StartTalk(text);
        }

        public override void Stop()
        {
            context.Talk.CancelTalk();
        }

        public override void Pause()
        {
            context.Talk.PauseTalk();
        }

        public override void Unpause()
        {
            context.Talk.ResumeTalk();
        }

        public override Status Update()
        {
            if (context.Talk.IsTalking())
            {
                return Status.Running;
            }
            else
            {
                _currentDelay += Time.deltaTime;
                if (_currentDelay > delay)
                {
                    return Status.Success;
                }
                else
                {
                    return Status.Running;
                }

            }
        }
    }
}

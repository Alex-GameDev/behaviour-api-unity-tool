using System;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class UnityTimerDecorator : BehaviourTrees.DecoratorNode
    {
        public float TotalTime;

        float _currentTime;
        bool _childExecuted;

        public override void Start()
        {
            base.Start();
            _currentTime = 0f;
            _childExecuted = false;
        }

        public UnityTimerDecorator SetTotalTime(float time)
        {
            TotalTime = time;
            return this;
        }

        protected override Status UpdateStatus()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime < TotalTime) return Status.Running;

            if (m_childNode != null)
            {
                if (!_childExecuted)
                {
                    m_childNode.Start();
                    _childExecuted = true;
                }
                m_childNode.Update();
                return m_childNode.Status;
            }
            throw new NullReferenceException("ERROR: Child node is not defined");
        }

        public override void Stop()
        {
            base.Stop();
            if (_childExecuted) m_childNode?.Stop();
        }
    }

}

using System;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using BehaviourTrees;
    using Core;

    /// <summary>
    /// Timer decorator that used unity time instead of system time..
    /// </summary>
    public class UnityTimerDecorator : DecoratorNode
    {
        /// <summary>
        /// The time that the decorator waits.
        /// </summary>
        public float TotalTime;

        float _currentTime;
        bool _childExecuted;

        public override void Start()
        {
            base.Start();
            _currentTime = 0f;
            _childExecuted = false;
        }

        /// <summary>
        /// Set the time that the decorator waits.
        /// </summary>
        /// <param name="time">The amount of time.</param>
        /// <returns>The decorator itself.</returns>
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

        public override void Pause()
        {
            if (_childExecuted) m_childNode?.Pause();
        }

        public override void Unpause()
        {
            if (_childExecuted) m_childNode.Unpause();
        }
    }

}

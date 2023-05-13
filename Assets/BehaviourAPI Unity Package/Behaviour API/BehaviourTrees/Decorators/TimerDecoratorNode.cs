using System.Timers;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// Decorator that waits an amount of time to execute the child.
    /// </summary>
    public class TimerDecoratorNode : DecoratorNode
    {
        #region --------------------------------------- Private fields ---------------------------------------

        Timer _timer;

        bool _isTimeout;
        bool _childExecuted;

        #endregion

        #region ------------------------------------------- Fields -------------------------------------------

        /// <summary>
        /// The total time that the decorator waits to execute its child.
        /// </summary>
        public float Time;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        /// <summary>
        /// Set the <see cref="Time"/> value to <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The new time value.</param>
        /// <returns>The <see cref="TimerDecoratorNode"/> itself.</returns>
        public TimerDecoratorNode SetTime(float time)
        {
            Time = time;
            return this;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// <inheritdoc/>
        /// Starts the timer.
        /// </summary>
        public override void Start()
        {
            base.Start();
            _childExecuted = false;
            _timer = new Timer(Time * 1000);
            _timer.Elapsed += OnTimerElapsed;

            _isTimeout = false;
            _timer.Enabled = true;
            _timer.Start();
        }

        /// <summary>
        /// <inheritdoc/>
        /// If the timer elapsed this frame, starts the child executions. Then update it.
        /// </summary>
        /// <returns>Running if the child is not executing yet, its status otherwise.</returns>
        /// <exception cref="MissingChildException">If child is null.</exception>
        protected override Status UpdateStatus()
        {
            if (!_isTimeout) return Status.Running;

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
            throw new MissingChildException(this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// Reset the timer and stops the child if its executing.
        /// </summary>
        /// <exception cref="MissingChildException">If child is null.</exception>
        public override void Stop()
        {
            base.Stop();
            _isTimeout = false;
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Stop();
                _timer.Dispose();
            }

            if (_childExecuted)
            {
                if (m_childNode == null)
                    throw new MissingChildException(this);

                m_childNode.Stop();
            }
        }

        public override void Pause()
        {
            if (!_isTimeout)
                _timer.Stop();

            if (_childExecuted)
                m_childNode.Pause();
        }

        public override void Unpause()
        {
            if (!_isTimeout)
                _timer.Start();

            if (_childExecuted)
                m_childNode.Unpause();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs evt)
        {
            _isTimeout = true;
        }


        #endregion
    }
}

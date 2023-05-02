using System.Timers;

namespace BehaviourAPI.Core.Perceptions
{
    /// <summary>
    /// Perception that returns false until a determined amount of time passes.
    /// </summary>
    public class TimerPerception : Perception
    {
        /// <summary>
        /// The amount of time that must pass after the perception initializes to return true.
        /// </summary>
        public float Time;

        Timer _timer;

        bool _isTimeout;

        /// <summary>
        /// Create a new timer perception with an specified time.
        /// </summary>
        /// <param name="time">The time value.</param>
        public TimerPerception(float time)
        {
            Time = time;
            _timer = new Timer(time * 1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Initialize the internal timer.
        /// </summary>
        public override void Initialize()
        {
            _isTimeout = false;
            _timer.Enabled = true;
            _timer.Start();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>Returns true if the timer's time has elapsed.</returns>
        public override bool Check()
        {
            return _isTimeout;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Reset the timer.
        /// </summary>
        public override void Reset()
        {
            _isTimeout = false;
            _timer.Enabled = false;
            _timer.Stop();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs evt)
        {
            _isTimeout = true;
        }


        /// <summary>
        /// <inheritdoc/>
        /// (The context in Timer perceptions is not used).
        /// </summary>
        public override void SetExecutionContext(ExecutionContext context)
        {
            return;
        }
    }
}

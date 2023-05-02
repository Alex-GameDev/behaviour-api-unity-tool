using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.StateMachines
{
    /// <summary>
    /// State that checks its transitions depending on probabilities.
    /// </summary>
    public class ProbabilisticState : State
    {
        #region ------------------------------------------ Properties -----------------------------------------
        
        /// <summary>
        /// The last random value generated to check probabilities (only for debug).
        /// </summary>
        public double RandomValue { get; private set; }

        #endregion

        #region -------------------------------------- Private variables -------------------------------------

        static Random Random = new Random();

        Dictionary<Transition, float> _probabilities = new Dictionary<Transition, float>();

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        /// <summary>
        /// Set the probability of the transition. If the value specified is 0 or less, 
        /// the probability will be removed.
        /// </summary>
        /// <param name="transition">The specified transition.</param>
        /// <param name="probability">The new probablity of the transition.</param>
        public void SetProbability(Transition transition, float probability)
        {
            if (_transitions.Contains(transition))
            {
                if (probability > 0)
                {
                    _probabilities[transition] = probability;                    
                }
                else
                {
                    _probabilities.Remove(transition);
                }
            }
        }
        public override object Clone()
        {
            ProbabilisticState state = (ProbabilisticState)base.Clone();
            state._probabilities = new Dictionary<Transition, float>();
            return state;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// <inheritdoc/>
        /// Generates a random number and checks the transition corresponding to that number. 
        /// If a transition with no assigned probability is checked, it will be executed instead.
        /// </summary>
        protected override void CheckTransitions()
        {
            var totalProbability = Math.Max(_probabilities.Sum(p => p.Value), 1f);
            var probability = Random.NextDouble() * totalProbability;
            RandomValue = probability;
            var currentProbSum = 0f;
            Transition selectedTransition = null;

            for (int i = 0; i < _transitions.Count; i++)
            {
                // First select a transition with the generated number
                Transition transition = _transitions[i];
                if (transition == null) break;

                if (_probabilities.TryGetValue(transition, out float value))
                {
                    if (selectedTransition == null)
                    {
                        currentProbSum += value;
                        if (currentProbSum > probability)
                        {
                            selectedTransition = transition;
                        }
                    }
                }
                // The transitions without prob assigned has priority:
                else
                {
                    if (CheckTransition(transition))
                    {
                        _transitions[i]?.Perform();
                        return;
                    }
                }
            }
            if (selectedTransition != null)
            {
                if (CheckTransition(selectedTransition))
                {
                    selectedTransition.Perform();
                }
            }
        }

        /// <summary>
        /// Get the probability assigned to a transition.
        /// If the transition doesn't have a probability assigned, return 0.
        /// </summary>
        /// <param name="t">The specified transition.</param>
        /// <returns>The probability assigned to the transition.</returns>
        public float GetProbability(Transition t)
        {
            if(_probabilities.TryGetValue(t, out float value))
            {
                return value;
            }
            return 0f;
        }
        
        #endregion
    }
}

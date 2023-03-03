using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.StateMachines
{
    using Core;
    public class ProbabilisticState : State
    {
        Dictionary<Transition, float> _probabilities;  

        public double Prob { get; private set; }

        public ProbabilisticState()
        {
            _probabilities = new Dictionary<Transition, float>();
        }

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

        protected override void CheckTransitions()
        {
            var totalProbability = Math.Max(_probabilities.Sum(p => p.Value), 1f);
            var probability = MathUtilities.Random.NextDouble() * totalProbability;
            Prob = probability;
            var currentProbSum = 0f;
            Transition selectedTransition = null;

            for (int i = 0; i < _transitions.Count; i++)
            {
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
                // The transitions without prob assigned has priority
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

        public float GetProbability(Transition t)
        {
            return _probabilities[t];
        }

        public override object Clone()
        {
            ProbabilisticState state = (ProbabilisticState)base.Clone();
            state._probabilities = new Dictionary<Transition, float>();
            return state;
        }
    }
}

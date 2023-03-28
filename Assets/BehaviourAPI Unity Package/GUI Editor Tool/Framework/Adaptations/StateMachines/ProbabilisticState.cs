using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ProbabilisticState : StateMachines.ProbabilisticState, IActionAssignable, IBuildable
    {
        [SerializeField] List<float> probabilities = new List<float>();
        [SerializeReference] Action action;

        public Action ActionReference
        {
            get => action;
            set => action = value;
        }

        public override object Clone()
        {
            var copy = (ProbabilisticState)base.Clone();
            copy.action = (Action)action?.Clone();
            return copy;
        }

        public void Build(SystemData data)
        {
            if (action is IBuildable buildable) buildable.Build(data);
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Action = action;

            var count = Mathf.Min(_transitions.Count, probabilities.Count);

            for (int i = 0; i < count; i++)
            {
                if (probabilities[i] > 0)
                {
                    SetProbability(_transitions[i], probabilities[i]);
                }
            }
        }
    }
}
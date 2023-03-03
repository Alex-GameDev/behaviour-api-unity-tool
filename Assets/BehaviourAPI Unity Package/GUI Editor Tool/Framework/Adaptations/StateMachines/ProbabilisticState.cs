using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ProbabilisticState : StateMachines.ProbabilisticState, IActionAssignable
    {
        [SerializeField] List<float> probabilities = new List<float>();

        [SerializeReference] Action _action;

        public Action ActionReference
        {
            get => _action;
            set => _action = value;
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Action = _action;

            var count = Mathf.Min(_transitions.Count, probabilities.Count);

            for(int i = 0; i < count; i++)
            {
                if (probabilities[i] > 0)
                {
                    SetProbability(_transitions[i], probabilities[i]);
                }
            }
        }
    }
}
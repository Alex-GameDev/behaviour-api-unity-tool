using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.StateMachines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ProbabilisticState : StateMachines.ProbabilisticState
    {
        [SerializeReference] Action _action;

        [SerializeField] List<float> probabilities = new List<float>();

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            var count = Mathf.Min(_transitions.Count, probabilities.Count);

            for(int i = 0; i < count; i++)
            {
                if (probabilities[i] > 0)
                {
                    SetProbability(_transitions[i], probabilities[i]);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            Action = _action;
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }
}
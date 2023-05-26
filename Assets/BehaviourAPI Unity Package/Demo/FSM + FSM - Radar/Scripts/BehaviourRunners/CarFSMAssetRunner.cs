using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.UnityToolkit.GUIDesigner.Runtime;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class CarFSMAssetRunner : AssetBehaviourRunner, ICar
    {
        Rigidbody _rb;

        protected override void Init()
        {
            _rb = GetComponent<Rigidbody>();

            base.Init();
            IRadar radar = GameObject.FindGameObjectWithTag("Radar").GetComponent<IRadar>();

            var mainGraph = FindGraph("main");
            var speedUp = mainGraph.FindNode<StateTransition>("speed up");
            var speedDown = mainGraph.FindNode<StateTransition>("speed down");

            speedUp.Perception = new ExecutionStatusPerception(radar.GetBrokenState(), StatusFlags.Running);
            speedDown.Perception = new ExecutionStatusPerception(radar.GetWorkingState(), StatusFlags.Running);
        }

        public float GetSpeed() => _rb.velocity.magnitude;
    }

}
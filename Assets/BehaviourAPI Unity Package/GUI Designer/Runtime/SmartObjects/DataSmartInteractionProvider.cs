using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.UnityToolkit.GUIDesigner.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Runtime
{
    public abstract class DataSmartInteractionProvider : SmartInteractionProvider
    {
        public override Action GetInteractionAction(SmartAgent agent)
        {
            SystemData data = GetSystemdata();
            BuildedSystemData buildedData = data.BuildSystem(agent);
            ModifyGraph(buildedData.GraphMap, buildedData.PushPerceptionMap);
            return new SubsystemAction(buildedData.MainGraph);
        }

        protected abstract SystemData GetSystemdata();

        protected virtual void ModifyGraph(Dictionary<string, BehaviourGraph> graphs, Dictionary<string, PushPerception> pushPerceptionMap)
        {
            return;
        }

    }
}

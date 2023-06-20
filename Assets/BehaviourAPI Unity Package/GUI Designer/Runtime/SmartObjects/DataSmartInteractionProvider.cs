using System.Collections.Generic;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Runtime
{
    using Core;
    using Core.Actions;
    using Framework;

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

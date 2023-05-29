using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Runtime
{
    using Core.Actions;
    using Framework;

    public class GUISmartInteractionProvider : DataSmartInteractionProvider, IBehaviourSystem
    {
        [SerializeField] SystemData systemData;
        public SystemData Data => systemData;
        public Object ObjectReference => this;

        public override Action GetInteractionAction(SmartAgent agent)
        {
            BuildedSystemData buildedData = systemData.BuildSystem(agent);
            ModifyGraph(buildedData.GraphMap, buildedData.PushPerceptionMap);
            return new SubsystemAction(buildedData.MainGraph);
        }

        protected override SystemData GetSystemdata() => systemData;
    }
}

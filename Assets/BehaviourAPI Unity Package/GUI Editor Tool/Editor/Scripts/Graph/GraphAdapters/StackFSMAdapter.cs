using BehaviourAPI.StateMachines.StackFSMs;

namespace BehaviourAPI.Unity.Editor
{
    [CustomGraphAdapter(typeof(StackFSM))]
    public class StackFSMAdapter : StateMachineAdapter
    {
        public override string IconPath => BehaviourAPISettings.instance.IconPath + "Graphs/stackfsm.png";
    }
}

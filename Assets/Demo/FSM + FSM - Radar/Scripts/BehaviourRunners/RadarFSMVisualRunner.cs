using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Demo
{
    public class RadarFSMVisualRunner : VisualBehaviourRunner, IRadar
    {
        public State GetBrokenState()
        {
            return FindGraph("Main").FindNode<State>("broken_state");
        }

        public State GetWorkingState()
        {
            return FindGraph("Main").FindNode<State>("working_state");
        }
    }
}
using BehaviourAPI.StateMachines;

namespace BehaviourAPI.Unity.Demos
{
	public interface IRadar
	{
		public State GetBrokenState();
		public State GetWorkingState();
	}

}
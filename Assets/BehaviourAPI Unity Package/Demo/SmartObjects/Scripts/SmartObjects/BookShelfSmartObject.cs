namespace BehaviourAPI.UnityToolkit.Demos
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.SmartObjects;
    using BehaviourAPI.UnityToolkit;

    public class BookShelfSmartObject : DirectSmartObject
    {
        public override bool ValidateAgent(SmartAgent agent)
        {
            // Comprobar si hay asientos libres
            return true;
        }

        protected override Action GetUseAction(SmartAgent agent, RequestData requestData)
        {
            var seatAction = new SeatRequestAction(agent);
            return seatAction;
        }
    }
}
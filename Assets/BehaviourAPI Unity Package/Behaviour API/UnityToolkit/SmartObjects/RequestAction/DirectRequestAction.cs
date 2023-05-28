using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit
{
    public class DirectRequestAction : UnityRequestAction
    {
        public SmartObject smartObject;

        public override string DisplayInfo => "Request to $smartObject";

        public DirectRequestAction() : base()
        {
        }

        public DirectRequestAction(SmartObject smartObject)
        {
            this.smartObject = smartObject;
        }

        public DirectRequestAction(SmartAgent agent, SmartObject smartObject) : base(agent)
        {
            this.smartObject = smartObject;
        }

        protected override ISmartObjectProvider<SmartAgent> GetSmartObjectProvider()
        {
            return new DirectSOProvider<SmartAgent>(smartObject);
        }

        protected override RequestData GetRequestData()
        {
            return new RequestData();
        }
    }
}

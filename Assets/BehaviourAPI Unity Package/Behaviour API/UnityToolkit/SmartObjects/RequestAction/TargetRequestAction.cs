using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit
{
    public class TargetRequestAction : UnityRequestAction
    {
        public SmartObject smartObject;

        public RequestData requestData;

        public TargetRequestAction(SmartAgent agent, SmartObject smartObject, RequestData requestData) : base(agent)
        {
            this.smartObject = smartObject;
            this.requestData = requestData;
        }

        public TargetRequestAction()
        {
        }

        protected override SmartObject GetRequestedSmartObject() => smartObject;

        protected override RequestData GetRequestData() => requestData;
    }
}

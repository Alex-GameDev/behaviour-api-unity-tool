using BehaviourAPI.Unity.SmartObjects;

namespace BehaviourAPI.UnityExtensions
{
    public class DirectRequestAction : UnityRequestAction
    {
        public SmartObject smartObject;

        public DirectRequestAction() : base()
        {
        }

        public DirectRequestAction(SmartAgent agent, SmartObject smartObject) : base(agent)
        {
            this.smartObject = smartObject;
        }

        protected override SmartObject GetSmartObject(SmartAgent agent)
        {
            return smartObject;
        }
    }
}

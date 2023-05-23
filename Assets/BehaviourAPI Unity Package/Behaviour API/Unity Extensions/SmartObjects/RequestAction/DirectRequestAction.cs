using BehaviourAPI.SmartObjects;
using BehaviourAPI.Unity.SmartObjects;

namespace BehaviourAPI.UnityExtensions
{
    public class DirectRequestAction : UnityRequestAction
    {
        public SmartObject smartObject;

        public override string DisplayInfo => "Request to $smartObject";

        public DirectRequestAction() : base()
        {
        }

        public DirectRequestAction(SmartAgent agent, SmartObject smartObject, string interactionName = null) : base(agent)
        {
            this.smartObject = smartObject;
        }

        protected override SmartObject GetSmartObject(SmartAgent agent)
        {
            return smartObject;
        }
    }
}

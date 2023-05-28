using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit
{
    using Core.Actions;

    /// <summary>
    /// Smart object that provide allways the same interaction
    /// </summary>
    public abstract class SimpleSmartObject : SmartObject
    {
        public override SmartInteraction RequestInteraction(SmartAgent agent, RequestData requestData)
        {
            Action action = GenerateAction(agent, requestData);
            SmartInteraction interaction = new SmartInteraction(action, agent, GetCapabilities());
            SetInteractionEvents(interaction, agent);
            return interaction;
        }

        protected abstract Action GenerateAction(SmartAgent agent, RequestData requestData);

        protected virtual void SetInteractionEvents(SmartInteraction interaction, SmartAgent agent)
        {
            return;
        }
    }
}

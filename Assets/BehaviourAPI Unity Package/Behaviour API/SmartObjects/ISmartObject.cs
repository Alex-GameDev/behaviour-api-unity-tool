namespace BehaviourAPI.SmartObjects
{
    public interface ISmartObject
    {
        void RequestInteraction();

        float GetCapability(string name);

        bool ValidateAgent();
    }
}

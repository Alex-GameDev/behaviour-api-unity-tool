using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;

public class UnityTypedRequestAction<T> : UnityRequestAction
{
    public UnityTypedRequestAction()
    {
    }

    public UnityTypedRequestAction(SmartAgent agent) : base(agent)
    {
    }

    protected override SmartObject GetSmartObject(SmartAgent agent)
    {
        var ovens = SmartObjectManager.Instance.RegisteredObjects.Find(obj => obj is T);
        return ovens;
    }
}

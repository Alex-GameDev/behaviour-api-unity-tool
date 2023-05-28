namespace BehaviourAPI.SmartObjects
{
    /// <summary>
    /// Class that provides a SmartObject to a request action.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public interface ISmartObjectProvider<T> where T : ISmartAgent
    {
        public abstract ISmartObject<T> GetSmartObject(T agent);
    }
}

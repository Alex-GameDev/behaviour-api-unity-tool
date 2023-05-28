using System.Collections.Generic;

namespace BehaviourAPI.SmartObjects
{
    public abstract class CollectionSOProvider<T> : ISmartObjectProvider<T> where T : ISmartAgent
    {
        public IEnumerable<ISmartObject<T>> SmartObjectPool { get; set; }

        public ISmartObject<T> GetSmartObject(T agent) => GetObjectFromPool(agent, SmartObjectPool);

        protected abstract ISmartObject<T> GetObjectFromPool(T agent, IEnumerable<ISmartObject<T>> pool);
    }
}

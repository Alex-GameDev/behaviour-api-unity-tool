using System.Collections.Generic;

namespace BehaviourAPI.SmartObjects
{
    /// <summary>
    /// Provides the element that 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CapabilitySOProvider<T> : CollectionSOProvider<T> where T : ISmartAgent
    {
        public string CapabilityName { get; set; }

        protected override ISmartObject<T> GetObjectFromPool(T agent, IEnumerable<ISmartObject<T>> pool)
        {
            ISmartObject<T> currentItem = null;
            float currentValue = 0;
            foreach (var item in pool) 
            {
                float itemvalue = item.GetCapabilityValue(CapabilityName);
                if (item.GetCapabilityValue(CapabilityName) > currentValue)
                {
                    currentItem = item;
                    currentValue = itemvalue;
                }
            }
            return currentItem;
        }
    }
}

using System;
using System.Collections.Generic;

namespace BehaviourAPI.SmartObjects
{
    /// <summary>
    /// Class that allows to store a collection of smart objects and operate on it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartObjectPool<T> where T : ISmartAgent
    {
        public IEnumerable<ISmartObject<T>> Objects { get; private set; }

        protected SmartObjectPool(IEnumerable<ISmartObject<T>> objects)
        {
            Objects = objects;
        }

        /// <summary>
        /// Returns the largest capacity value contained in the pool for a specific need.
        /// </summary>
        /// <param name="needName">The name of the need</param>
        /// <returns>The maximum value of the capacity.</returns>
        public float GetCapability(string needName)
        {
            float currentValue = 0f;
            foreach (var obj in Objects) 
            { 
                currentValue = MathF.Max(currentValue, obj.GetCapabilityValue(needName));
            }
            return currentValue;
        }

        /// <summary>
        /// Returns the largest capacity value contained in the pool for a specific need, and
        /// the object that provides it.
        /// </summary>
        /// <param name="needName">The name of the need</param>
        /// <returns>The maximum value of the capacity.</returns>
        public float GetCapability(string needName, out ISmartObject<T> smartObject)
        {
            float currentValue = 0f;
            smartObject = null;
            foreach (var obj in Objects)
            {
                float value = obj.GetCapabilityValue(needName);
                if(value > currentValue)
                {
                    currentValue = value;
                    smartObject = obj;
                }
            }
            return currentValue;
        }
    }
}

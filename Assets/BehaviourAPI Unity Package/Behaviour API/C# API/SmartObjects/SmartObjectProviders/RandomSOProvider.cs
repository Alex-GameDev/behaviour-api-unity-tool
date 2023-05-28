using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.SmartObjects
{
    /// <summary>
    /// Provider that gets a random object from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the agent</typeparam>
    public class RandomSOProvider<T> : CollectionSOProvider<T> where T : ISmartAgent
    {
        private Random random = new Random();

        protected override ISmartObject<T> GetObjectFromPool(T agent, IEnumerable<ISmartObject<T>> pool)
        {
            if (pool.Count() == 0) return null;

            int randomIndex = random.Next(pool.Count());
            var enumerator = pool.GetEnumerator();

            for (int i = 0; i <= randomIndex; i++)
            {
                enumerator.MoveNext();
            }

            return enumerator.Current;
        }
    }
}

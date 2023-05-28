using System;

namespace BehaviourAPI.SmartObjects
{
    public class DirectSOProvider<T> : ISmartObjectProvider<T> where T : ISmartAgent
    {
        public ISmartObject<T> smartObject { get; set; }

        public DirectSOProvider(ISmartObject<T> smartObject)
        {
            this.smartObject = smartObject;
        }

        public ISmartObject<T> GetSmartObject(T agent) => smartObject;
    }
}

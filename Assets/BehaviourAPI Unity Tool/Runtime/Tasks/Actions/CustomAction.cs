using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System;
using System.Linq.Expressions;
using UnityEngine;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomAction : Action, ISerializationCallbackReceiver
    {
        public Component component;
        public string methodName;

        Func<Status> updateFunc;

        public void OnAfterDeserialize()
        {
            if (component != null)
            {

                if (component.GetType().GetMethod(methodName) == null)
                {
                    methodName = "";
                    updateFunc = null;
                }
                else if (updateFunc == null)
                {
                    var method = component.GetType().GetMethod(methodName);
                    updateFunc = Expression.Lambda<Func<Status>>(Expression.Call(Expression.Constant(component), method)).Compile();
                }
            }
        }

        public void OnBeforeSerialize()
        {
            return;
        }

        public override void Start()
        {
           if(updateFunc == null)
           {
               
           }
        }

        public override void Stop()
        {
           
        }

        public override Status Update()
        {
            if(updateFunc == null) throw new MissingMethodException();
            return updateFunc.Invoke();
        }
    }
}

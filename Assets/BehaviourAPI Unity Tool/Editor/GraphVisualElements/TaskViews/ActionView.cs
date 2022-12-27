using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class ActionView<T> : VisualElement where T : Action
    {
        protected T _action;

        public ActionView(T action, VisualTreeAsset layoutAsset)
        {
            _action = action;

            Add(layoutAsset.Instantiate());
            AddLayout();
        }

        protected abstract void AddLayout();
    }
}

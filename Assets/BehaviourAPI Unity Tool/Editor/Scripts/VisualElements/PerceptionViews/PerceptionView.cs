using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class PerceptionView<T> : VisualElement where T : Perception
    {
        protected T _perception;

        public PerceptionView(T action, VisualTreeAsset layoutAsset)
        {
            _perception = action;

            Add(layoutAsset.Instantiate());
            AddLayout();
        }

        protected abstract void AddLayout();
    }
}

using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class PerceptionInspectorView : ListInspectorView<PerceptionAsset>
    {

        public PerceptionInspectorView(BehaviourSystemAsset systemAsset) : base(systemAsset, "Perceptions", Side.Right) { }

        protected override List<PerceptionAsset> GetList() => _systemAsset.Perceptions;

        protected override void RemoveElement(PerceptionAsset asset) => _systemAsset.RemovePerception(asset);

        protected override void AddLayout()
        {
            base.AddLayout();
            _mainContainer.Add(new Button(CreateCustomPerception) { text = "Create custom perception" });
            _mainContainer.Add(new Button(CreateUnityPerception) { text = "Create unity perception" });
        }          

        void CreateCustomPerception()
        {
            var perception = _systemAsset.CreatePerception("new custom perception", typeof(CustomPerceptionAsset));
            OnCreateElement?.Invoke(perception);
            RefreshList();
        }

        void CreateUnityPerception()
        {

        }
    }
}

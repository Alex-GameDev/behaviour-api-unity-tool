using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ActionInspectorView : ListInspectorView<ActionAsset>
    {
        public ActionInspectorView(BehaviourSystemAsset systemAsset) : base(systemAsset, "Actions", Side.Right) { }

        protected override List<ActionAsset> GetList() => _systemAsset.Actions;

        protected override void RemoveElement(ActionAsset asset) => _systemAsset.RemoveAction(asset);

        protected override void AddLayout()
        {
            base.AddLayout();
            _mainContainer.Add(new Button(CreateCustomAction) { text = "Create custom action" });
            _mainContainer.Add(new Button(CreateUnityAction) { text = "Create unity action" });
        }      

        void CreateCustomAction()
        {
            var action = _systemAsset.CreateAction("new custom action", typeof(CustomActionAsset));
            OnCreateElement?.Invoke(action);
            RefreshList();
        }

        void CreateUnityAction()
        {

        }
    }
}

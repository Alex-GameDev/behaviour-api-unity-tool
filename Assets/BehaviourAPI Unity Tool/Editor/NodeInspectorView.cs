using BehaviourAPI.Unity.Runtime;
using System;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class NodeInspectorView : VisualElement
    {
        public NodeInspectorView()
        {
            AddLayout();
            AddStyles();
        }

        private void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().InspectorLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);
        }

        private void AddStyles()
        {
            var styleSheet = VisualSettings.GetOrCreateSettings().InspectorStylesheet;
            styleSheets.Add(styleSheet);
        }


        public void UpdateInspector(NodeAsset nodeAsset)
        {

        }
    }
}

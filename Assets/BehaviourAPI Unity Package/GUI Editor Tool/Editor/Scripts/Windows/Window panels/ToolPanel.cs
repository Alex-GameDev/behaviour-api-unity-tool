using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ToolPanel : VisualElement
    {
        bool canClose = true;

        public ToolPanel(string relativePath)
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BehaviourAPISettings.instance.EditorLayoutsPath + relativePath);
            asset.CloneTree(this);
            this.StretchToParentSize();

            Button closeBtn = this.Q<Button>("cgp-close-btn");
            closeBtn.clicked += ClosePanel;
        }

        public void ClosePanel()
        {
            if (canClose)
            {
                this.Disable();
            }
        }

        public void Open(bool canClose = true)
        {
            this.canClose = canClose;
            this.Enable();
        }
    }
}

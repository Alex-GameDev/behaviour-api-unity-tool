using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class GenerateScriptPanel : VisualElement
    {
        public GenerateScriptPanel(SystemData data)
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BehaviourAPISettings.instance.EditorLayoutsPath + "/creategraphpanel.uxml");
            asset.CloneTree(this);
            this.StretchToParentSize();
        }
    }
}

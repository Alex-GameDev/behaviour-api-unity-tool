using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    public class GraphCreationWindow : EditorWindow
    {
        public static Action<string, Type> OnPressCreate;

        public static void Create(Action<string, Type> onPressCreate)
        {
            OnPressCreate = onPressCreate;

            GraphCreationWindow wnd = GetWindow<GraphCreationWindow>();
            wnd.titleContent = new GUIContent("Create graph");

            wnd.minSize = new Vector2(300, 300);
            wnd.maxSize = new Vector2(300, 300);

            wnd.ShowModalUtility();
        }

        public void CreateGUI()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().GraphCreationWindowLayout;
            rootVisualElement.styleSheets.Add(VisualSettings.GetOrCreateSettings().BehaviourGraphEditorWindowStylesheet);
            VisualElement labelFromUXML = visualTree.Instantiate();
            rootVisualElement.Add(labelFromUXML);

            var graphNameInputText = rootVisualElement.Q<TextField>("cgw-name-textfield");
            var createGraphList = rootVisualElement.Q<ScrollView>("cgw-graphs-scrollview");

            typeof(BehaviourGraph).GetSubClasses().ForEach(gType => createGraphList
                .Add(new Button(() => { 
                    OnPressCreate?.Invoke(graphNameInputText.value, gType); 
                    Close(); 
                }) 
                { 
                    text = gType.Name 
                })
            );
        }
    }
}

using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    public class GraphCreationWindow : EditorWindow
    {
        private static string inspectorPath => BehaviourAPISettings.instance.EditorLayoutsPath + "/windows/creategraphwindow.uxml";

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
            var windownFromUXML =  AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(inspectorPath).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            var graphNameInputText = rootVisualElement.Q<TextField>("cgw-name-textfield");
            var createGraphList = rootVisualElement.Q<ScrollView>("cgw-graphs-scrollview");

            typeof(GraphAdapter).GetSubClasses().ForEach(adapterType =>
            {
                var adapterAttribute = adapterType.GetCustomAttribute<CustomAdapterAttribute>();
                if (adapterAttribute != null)
                {
                    var graphType = adapterAttribute.type;
                    if(graphType.IsSubclassOf(typeof(BehaviourGraph)))
                    {
                        createGraphList.Add(new Button(() =>
                        {
                            OnPressCreate?.Invoke(graphNameInputText.value, graphType);
                            Close();
                        })
                        {
                            text = graphType.Name
                        });
                    }
                }
            });
        }
    }
}

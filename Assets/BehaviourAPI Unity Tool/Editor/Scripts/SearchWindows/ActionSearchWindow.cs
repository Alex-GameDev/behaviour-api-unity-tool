using BehaviourAPI.Unity.Runtime;
using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class ActionSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        Action<Type> _entrySelected;

        Texture2D _defaultIdentationIcon;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();
            list.Add(CreateGroup("Actions", 0));
            list.Add(CreateEntry(typeof(CustomAction), 1));

            list.Add(CreateGroup("UnityActions", 1));
            var unityActionTypes = typeof(UnityAction).GetSubClasses().FindAll(t => !t.IsAbstract);
            unityActionTypes.ForEach(t => list.Add(CreateEntry(t, 2)));

            list.Add(CreateEntry(typeof(SubgraphAction), 1));
            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _entrySelected?.Invoke((Type)SearchTreeEntry.userData);
            _entrySelected = null;
            return true;
        }


        SearchTreeEntry CreateEntry(Type type, int level)
        {
            return new SearchTreeEntry(new GUIContent($" {type.Name}", GetDefaultIdentationIcon()))
            {
                level = level,
                userData = type
            };
        }

        SearchTreeGroupEntry CreateGroup(string name, int level)
        {
            return new SearchTreeGroupEntry(new GUIContent(name), level);
        }

        Texture2D GetDefaultIdentationIcon()
        {
            if(_defaultIdentationIcon == null)
            {
                _defaultIdentationIcon = new Texture2D(1, 1);
                _defaultIdentationIcon.SetPixel(0, 0, Color.clear);
                _defaultIdentationIcon.Apply();
            }
            return _defaultIdentationIcon;
        }

        public void Open(Action<Type> callback)
        {
            var searchContext = new SearchWindowContext();
            _entrySelected = callback;
            SearchWindow.Open(searchContext, this);
        }
    }
}

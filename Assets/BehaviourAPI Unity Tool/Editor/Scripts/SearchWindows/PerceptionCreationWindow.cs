using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class PerceptionCreationWindow : ScriptableObject, ISearchWindowProvider
    {
        Action<Type> _entrySelected;

        Texture2D _defaultIdentationIcon;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();
            list.Add(CreateGroup("Perceptions", 0));
            list.Add(CreateEntry(typeof(CustomPerception), 1));

            list.Add(CreateGroup("Unity Perceptions", 1));
            var unityPerceptionTypes = typeof(UnityPerception).GetSubClasses().FindAll(t => !t.IsAbstract);
            unityPerceptionTypes.ForEach(t => list.Add(CreateEntry(t, 2)));

            list.Add(CreateGroup("Compound Perceptions", 1));
            var compoundPerceptionTypes = typeof(CompoundPerception).GetSubClasses().FindAll(t => !t.IsAbstract);
            compoundPerceptionTypes.ForEach(t => list.Add(CreateEntry(t, 2)));

            list.Add(CreateEntry(typeof(ExecutionStatusPerception), 1));
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
            if (_defaultIdentationIcon == null)
            {
                _defaultIdentationIcon = new Texture2D(1, 1);
                _defaultIdentationIcon.SetPixel(0, 0, Color.clear);
                _defaultIdentationIcon.Apply();
            }
            return _defaultIdentationIcon;
        }

        public void Open(Action<Type> callback)
        {
            _entrySelected = callback;
            var mousePos = Event.current.mousePosition;
            mousePos += BehaviourSystemEditorWindow.Instance.position.position;
            SearchWindow.Open(new SearchWindowContext(mousePos), this);
        }

        public static PerceptionCreationWindow Create() => CreateInstance<PerceptionCreationWindow>();
    }
}

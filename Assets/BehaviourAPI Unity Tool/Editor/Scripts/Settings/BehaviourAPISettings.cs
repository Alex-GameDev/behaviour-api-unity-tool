using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [FilePath("Config/StateFile.foo", FilePathAttribute.Location.PreferencesFolder)]
    public class BehaviourAPISettings : ScriptableSingleton<BehaviourAPISettings>
    {
        #region ----------------------- Script generation -----------------------

        public string GenerateScriptDefaultPath = "Assets/Scripts/";
        public string GenerateScriptDefaultName = "NewBehaviourRunner";

        #endregion

        public string RootPath = "Assets/BehaviourAPI Unity Tool";
        public string EditorLayoutsPath => $"{RootPath}/Editor/uxml/";
        public string EditorStylesPath => $"{RootPath}/Editor/uss/";


        public string Assemblies;

        public readonly string[] DefaultAssemblies = new[]
        {
            "Assembly-CSharp",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.Unity.Framework",
            "BehaviourAPI.Unity.Editor",
            "BehaviourAPI.Core",
            "BehaviourAPI.StateMachines",
            "BehaviourAPI.BehaviourTrees",
            "BehaviourAPI.UtilitySystems"
        };

        public void Save() => Save(true);

        public List<Assembly> GetAssemblies()
        {
            return DefaultAssemblies.Select(assemblyName => Assembly.Load(assemblyName)).ToList();
        }

        public List<Type> GetTypes()
        {
            return DefaultAssemblies.Select(assemblyName => Assembly.Load(assemblyName)).SelectMany(a => a.GetTypes()).ToList();
        }
    }
}
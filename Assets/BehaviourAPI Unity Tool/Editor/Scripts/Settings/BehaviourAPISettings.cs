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

        public string CustomAssemblies;

        public readonly string[] DefaultAssemblies = new[]
        {
            "Assembly-CSharp",
            "BehaviourAPI.Core",
            "BehaviourAPI.StateMachines",
            "BehaviourAPI.BehaviourTrees",
            "BehaviourAPI.UtilitySystems",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.Unity.Framework",
            "BehaviourAPI.Unity.Editor"
        };

        List<Assembly> assemblies = new List<Assembly>();

        public void Save() => Save(true);

        public List<Assembly> GetAssemblies() => assemblies;

        public List<Type> GetTypes()
        {
            return GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
        }

        public void ReloadAssemblies()
        {
            var customAssemblies = CustomAssemblies.Split(';').ToList();
            var allAssemblyNames = customAssemblies.Union(DefaultAssemblies).ToHashSet();
            assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList().FindAll(assembly =>
                allAssemblyNames.Contains(assembly.GetName().Name));
        }
    }
}
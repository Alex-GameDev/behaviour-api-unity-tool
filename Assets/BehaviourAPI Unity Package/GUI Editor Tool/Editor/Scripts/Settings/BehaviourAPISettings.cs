using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Core;
    using Core.Perceptions;
    using Core.Actions;
    using Framework.Adaptations;
    using UnityExtensions;
    using Codice.Client.Common;

    [FilePath("ProjectSettings/BehaviourAPISettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BehaviourAPISettings : ScriptableSingleton<BehaviourAPISettings>
    {
        #region ------------------------- Default values ------------------------

        private static readonly Color k_LeafNodeColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_DecoratorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_CompositeColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_StateColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_TransitionColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_LeafFactorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_CurveFactorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_FusionFactorColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_SelectableNodeColor = new Color(1f, 0.65f, 0.15f, 1f);
        private static readonly Color k_BucketColor = new Color(1f, 0.65f, 0.15f, 1f);

        private static readonly string[] k_DefaultAssemblies = new[]
        {
            "Assembly-CSharp",
            "BehaviourAPI.Core",
            "BehaviourAPI.StateMachines",
            "BehaviourAPI.BehaviourTrees",
            "BehaviourAPI.UtilitySystems",
            "BehaviourAPI.UnityExtensions",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.UnityTool.Framework",
            "BehaviourAPI.Unity.Editor"
        };

        private static readonly string k_RootPath = "Assets/BehaviourAPI Unity Package/GUI Editor Tool";
        #endregion

        #region ----------------------- Editor settings -----------------------

        [SerializeField] private string RootPath = k_RootPath;
        [SerializeField] private string CustomAssemblies = string.Empty;

        public string GenerateScriptDefaultPath = "Assets/Scripts/";
        public string GenerateScriptDefaultName = "NewBehaviourRunner";

        [Header("Colors")]
        public Color LeafNodeColor = k_LeafNodeColor;
        public Color DecoratorColor = k_DecoratorColor;
        public Color CompositeColor = k_CompositeColor;

        public Color StateColor = k_StateColor;
        public Color TransitionColor = k_TransitionColor;

        public Color LeafFactorColor = k_LeafFactorColor;
        public Color CurveFactorColor = k_CurveFactorColor;
        public Color FusionFactorColor = k_FusionFactorColor;
        public Color SelectableNodeColor = k_SelectableNodeColor;
        public Color BucketColor = k_BucketColor;

        #endregion

        /// <summary>
        /// Root path of editor layout elements
        /// </summary>
        public string EditorLayoutsPath => $"{RootPath}/Editor/Resources/uxml/";

        /// <summary>
        /// Root path of editor style sheets
        /// </summary>
        public string EditorStylesPath => $"{RootPath}/Editor/Resources/uss/";

        /// <summary>
        /// Root path of the script templates
        /// </summary>
        public string ScriptTemplatePath => $"{RootPath}/Editor/Resources/Templates/";

        /// <summary>
        /// 
        /// </summary>
        public string IconPath => $"{RootPath}/Editor/Resources/Icons/";

        /// <summary>
        /// 
        /// </summary>
        public APITypeMetadata Metadata;

        public void Save() => Save(true);

        /// <summary>
        /// Create the type hierarchies used in the editor when Unity reloads.
        /// </summary>
        public void ReloadAssemblies()
        {
            Metadata = new APITypeMetadata();

            if (!System.IO.Directory.Exists(RootPath))
            {
                Debug.LogWarning("BehaviourAPISettings: Root path doesn't exist. Change the path in ProyectSetting > BehaviourAPI");
            }
        }       
    }
}
using System;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class GenerateScriptPanel : ToolPanel
    {
        private TextField m_ScriptText;
        private TextField m_classNameText;
        private TextField m_pathText;

        private Toggle m_IncludeNodeNamesToggle;
        CustomEditorWindow m_window;


        public GenerateScriptPanel(CustomEditorWindow customEditorWindow) : base("/generatescriptpanel.uxml")
        {
            m_window = customEditorWindow;
            m_ScriptText = this.Q<TextField>("gsp-text");
            m_classNameText = this.Q<TextField>("gsp-name-field");
            m_pathText = this.Q<TextField>("gsp-path-field");

            var generateBtn = this.Q<Button>("gsp-generate-btn");
            generateBtn.clicked += GenerateCode;

            var createBtn = this.Q<Button>("gsp-create-btn");
            createBtn.clicked += GenerateScriptAsset;

            m_IncludeNodeNamesToggle = this.Q<Toggle>("csw-includenodename-toggle");
        }

        private void GenerateCode()
        {
            bool includeNodeNames = m_IncludeNodeNamesToggle.value;
            m_ScriptText.value = ScriptGeneration.GenerateSystemCode(m_window.System, m_classNameText.value, true, includeNodeNames);
        }

        private void GenerateScriptAsset()
        {
            bool includeNodeNames = m_IncludeNodeNamesToggle.value;
            ScriptGeneration.GenerateScript(m_pathText.value, m_classNameText.value, m_window.System, true, includeNodeNames);
        }
    }
}

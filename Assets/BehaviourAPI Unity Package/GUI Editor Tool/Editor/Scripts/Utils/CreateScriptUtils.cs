using UnityEditor;

namespace BehaviourAPI.Unity.Editor
{
    public static class CreateScriptUtils
    {
        [MenuItem("Assets/Create/BehaviourAPI/CodeBehaviourRunner")]
        public static void CreateCodeBehaviourRunner()
        {
            string templatePath = BehaviourAPISettings.instance.ScriptTemplatePath + "/CodeRunnerTemplate.cs.txt";

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewCodeBehaviourRunner.cs");
        }

        [MenuItem("Assets/Create/BehaviourAPI/EditorBehaviourRunner")]
        public static void CreateEditorBehaviourRunner()
        {
            string templatePath = BehaviourAPISettings.instance.ScriptTemplatePath + "/EditorRunnerTemplate.cs.txt";

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewEditorBehaviourRunner.cs");
        }

        [MenuItem("Assets/Create/BehaviourAPI/AssetBehaviourRunner")]
        public static void CreateAssetBehaviourRunner()
        {
            string templatePath = BehaviourAPISettings.instance.ScriptTemplatePath + "/AssetRunnerTemplate.cs.txt";

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewAssetBehaviourRunner.cs");
        }

        [MenuItem("Assets/Create/BehaviourAPI/UnityAction")]
        public static void CreateUnityAction()
        {
            string templatePath = BehaviourAPISettings.instance.ScriptTemplatePath + "/UnityActionTemplate.cs.txt";

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewUnityAction.cs");
        }

        [MenuItem("Assets/Create/BehaviourAPI/UnityPerception")]
        public static void CreateUnityPerception()
        {
            string templatePath = BehaviourAPISettings.instance.ScriptTemplatePath + "/UnityPerceptionTemplate.cs.txt";

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewUnityPerception.cs");
        }
    }
}

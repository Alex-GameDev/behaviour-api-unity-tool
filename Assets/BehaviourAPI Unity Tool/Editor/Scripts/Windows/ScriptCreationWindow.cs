using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ScriptCreationWindow : EditorWindow
    {
        TextField scriptNameTextField, pathTextField;
        private static string path => BehaviourAPISettings.instance.EditorLayoutsPath + "/Windows/createscriptwindow.uxml";

        public static void Create()
        {
            ScriptCreationWindow wnd = GetWindow<ScriptCreationWindow>();
            wnd.titleContent = new GUIContent("Create script");

            wnd.minSize = new Vector2(400, 300);
            wnd.maxSize = new Vector2(400, 300);

            wnd.ShowModalUtility();
        }

        public void CreateGUI()
        {

            var windownFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            scriptNameTextField = rootVisualElement.Q<TextField>("csw-name-textfield");
            pathTextField = rootVisualElement.Q<TextField>("csw-path-textfield");

            scriptNameTextField.value = BehaviourAPISettings.instance.GenerateScriptDefaultName;
            pathTextField.value = BehaviourAPISettings.instance.GenerateScriptDefaultPath;
            rootVisualElement.Q<Button>("csw-create-button").clicked += GenerateScript;
        }

        void GenerateScript()
        {
            string path = pathTextField.text;
            string scriptName = scriptNameTextField.text;

            ScriptGeneration.GenerateScript(path, scriptName, BehaviourGraphEditorWindow.SystemAsset);
            Close();
        }
    }
}

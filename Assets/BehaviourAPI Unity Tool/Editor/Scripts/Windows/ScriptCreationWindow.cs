using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ScriptCreationWindow : EditorWindow
    {
        TextField scriptNameTextField, pathTextField;
        public static void Create()
        {
            ScriptCreationWindow wnd = new ScriptCreationWindow();
            wnd.titleContent = new GUIContent("Create script");

            wnd.minSize = new Vector2(300, 300);
            wnd.maxSize = new Vector2(300, 300);

            wnd.ShowModalUtility();
        }

        public void CreateGUI()
        {
            var visualTree = BehaviourAPISettings.instance.ScriptCreationWindowLayout;
            rootVisualElement.Add(visualTree.Instantiate());
            scriptNameTextField = rootVisualElement.Q<TextField>("csw-name-textfield");
            pathTextField = rootVisualElement.Q<TextField>("csw-path-textfield");

            scriptNameTextField.value = BehaviourAPISettings.instance.GenerateScript_DefaultName;
            pathTextField.value = BehaviourAPISettings.instance.GenerateScript_DefaultPath;
            rootVisualElement.Q<Button>("csw-create-button").clicked += GenerateScript;
        }

        void GenerateScript()
        {
            string path = pathTextField.text;
            string scriptName = scriptNameTextField.text;

            Debug.Log($"CreateScript: {path}{scriptName}");
            Close();
        }
    }
}

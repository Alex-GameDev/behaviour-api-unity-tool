using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.PackageManager.UI;
using System;

namespace BehaviourAPI.Unity.Editor
{
    public class AlertWindow : EditorWindow
    {
        public static string Question;

        public static Action OnPressYes, OnPressNo;
        public static void CreateAlertWindow(string question, Action onPressYes, Action onPressNo = null)
        {
            Question = question;
            OnPressYes = onPressYes;
            OnPressNo = onPressNo;

            AlertWindow wnd = GetWindow<AlertWindow>();
            wnd.titleContent = new GUIContent("AlertWindow");

            wnd.minSize = new Vector2(300, 150);
            wnd.maxSize = new Vector2(300, 150);

            wnd.ShowModalUtility();

        }

        public void CreateGUI()
        {
            Debug.Log("C");
            var visualTree = VisualSettings.GetOrCreateSettings().AlertWindowLayout;
            VisualElement labelFromUXML = visualTree.Instantiate();
            rootVisualElement.Add(labelFromUXML);

            var label = rootVisualElement.Q<Label>("aw-question-label");
            var yesBtn = rootVisualElement.Q<Button>("aw-yes-btn");
            var noBtn = rootVisualElement.Q<Button>("aw-no-btn");

            label.text = Question;
            yesBtn.clicked += () => { OnPressYes?.Invoke(); Close(); };
            noBtn.clicked += () => { OnPressNo?.Invoke(); Close(); };
        }
    }
}
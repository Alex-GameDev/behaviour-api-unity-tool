using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BehaviourAPI.UnityExtensions
{
    public class TextTalkComponent : MonoBehaviour, ITalkComponent
    {
        [SerializeField] Text textComponent;

        [SerializeField] bool _displayInmediate = false;

        [SerializeField] float _textSpeed = 30f;
        public bool IsInmediate { get => _displayInmediate; set => _displayInmediate = value; }

        bool isTalking;
        IEnumerator m_currentCoroutine;

        string m_currentText;

        bool isPaused;

        public void CancelTalk()
        {
            textComponent.text = "";
            StopCoroutine(m_currentCoroutine);
            m_currentCoroutine = null;
        }

        public void FinishCurrentTalkLine()
        {
            textComponent.text = m_currentText;
            StopCoroutine(m_currentCoroutine);
        }

        public bool IsTalking()
        {
            return isTalking;
        }

        public void StartTalk(string text)
        {
            m_currentText = text;
            m_currentCoroutine = DisplayText(text);
            StartCoroutine(m_currentCoroutine);
        }

        private IEnumerator DisplayText(string text)
        {
            isTalking = true;
            textComponent.text = "";

            if (_textSpeed <= 0) _textSpeed = 1f;
            float delay = 1f / _textSpeed;

            int i = 0;
            while (i < text.Length)
            {
                if (isPaused)
                {
                    yield return null;
                }
                else
                {
                    textComponent.text += text[i];
                    i++;
                    yield return new WaitForSeconds(delay);
                }
            }
            isTalking = false;
        }

        private void OnDisable()
        {
            Debug.Log("Disable component");
        }

        public void PauseTalk()
        {
            Debug.Log("Pause task");
            isPaused = true;
        }

        public void ResumeTalk()
        {
            Debug.Log("Unpause task");
            isPaused = false;
        }
    }
}

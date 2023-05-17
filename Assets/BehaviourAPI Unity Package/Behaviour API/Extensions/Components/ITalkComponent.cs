using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    /// <summary>
    /// Implement this interface to create a component that 
    /// </summary>
    public interface ITalkComponent
    {
        bool IsInmediate { get; set; }

        void StartTalk(string text);

        bool IsTalking();

        void FinishCurrentTalkLine();

        void CancelTalk();
    }
}

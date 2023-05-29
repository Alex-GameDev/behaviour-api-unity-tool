using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    public interface ISoundComponent
    {
        public void StartSound(AudioClip clip);

        public bool IsPlayingSound();

        public void CancelSound();

    }
}

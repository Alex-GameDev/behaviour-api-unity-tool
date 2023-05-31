using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    [SelectionGroup("SPRITE")]
    public class ChangeSpriteAction : UnityAction
    {
        [SerializeField] Sprite sprite;

        [SerializeField] float time = 1f;

        private Sprite _previousSprite;
        private float _currentTime;

        public override void Start()
        {
            _previousSprite = context.Renderer.Sprite;
            context.Renderer.Sprite = sprite;
        }

        public override Status Update()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > time)
            {
                if (time > 0f)
                {
                    context.Renderer.Sprite = _previousSprite;
                }
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }
}

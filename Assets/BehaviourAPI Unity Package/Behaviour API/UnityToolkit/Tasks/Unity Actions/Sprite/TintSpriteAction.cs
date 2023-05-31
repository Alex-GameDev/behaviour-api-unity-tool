using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    [SelectionGroup("SPRITE")]
    public class TintSpriteAction : UnityAction
    {
        [SerializeField] Color tintColor;

        [SerializeField] float time = 1f;

        private Color _previousColor;
        private float _currentTime;

        public override void Start()
        {
            _previousColor = context.Renderer.Tint;
            context.Renderer.Tint = tintColor;
        }

        public override Status Update()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > time)
            {
                if (time > 0f)
                {
                    context.Renderer.Tint = _previousColor;
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

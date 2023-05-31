using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    [SelectionGroup("SPRITE")]
    public class FlipSpriteAction : UnityAction
    {
        [SerializeField] bool flipx;
        [SerializeField] bool flipy;

        [SerializeField] float time = 1f;
        
        private float _currentTime;
        private bool previousFlipX;
        private bool previousFlipY;

        public override void Start()
        {
            previousFlipX = context.Renderer.FlipX;
            previousFlipY = context.Renderer.FlipY;

            context.Renderer.FlipX = flipx;
            context.Renderer.FlipY = flipy;
        }

        public override Status Update()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > time)
            {
                if(time > 0f) 
                {
                    context.Renderer.FlipX = previousFlipX;
                    context.Renderer.FlipY = previousFlipY;
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

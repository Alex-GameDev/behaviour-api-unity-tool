using UnityEngine;

public class NPCDisplay : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;

    public void SetSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.enabled = true;
    }

    public void UnsetSprite()
    {
        _spriteRenderer.sprite = null;
        _spriteRenderer.enabled = false;
    }
}

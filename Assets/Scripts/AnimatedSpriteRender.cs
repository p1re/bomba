using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedSpriteRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public Sprite idleSprite;
    public Sprite[] animationSprites;

    public float animationTime = 0.25f;
    private int animationFrame;

    public bool loop = true;
    public bool idle = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        spriteRenderer.enabled = true;
        // Al activarse, empezamos de cero
        animationFrame = -1; 
    }

    private void OnDisable()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }

    private void Start()
    {
        InvokeRepeating(nameof(NextFrame), animationTime, animationTime);
    }

    public void RestartAnimation()
    {
        animationFrame = -1;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }

    private void NextFrame()
    {
        animationFrame++;

        if (loop && animationFrame >= animationSprites.Length) {
            animationFrame = 0;
        }

        // Si no es bucle y hemos terminado, nos quedamos en el último frame
        if (!loop && animationFrame >= animationSprites.Length) {
            animationFrame = animationSprites.Length - 1;
            return;
        }

        if (idle) {
            spriteRenderer.sprite = idleSprite;
        } else if (animationFrame >= 0 && animationFrame < animationSprites.Length) {
            spriteRenderer.sprite = animationSprites[animationFrame];
        }
    }
}

using UnityEngine;

public class Explosion : MonoBehaviour
{
    public AnimatedSpriteRenderer start;
    public AnimatedSpriteRenderer middle;
    public AnimatedSpriteRenderer end;

    private Color paintColor;
    private bool shouldPaint = false;

    private void Awake()
    {
        // Asegurarse de que el objeto esté tageado y en la capa correcta
        gameObject.tag = "Explosion";
        gameObject.layer = LayerMask.NameToLayer("Explosion");

        // Asegurarse de que la explosión tenga un collider para dañar a los jugadores
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null) {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.8f);
    }

    public void SetActiveRenderer(AnimatedSpriteRenderer renderer)
    {
        start.enabled = renderer == start;
        middle.enabled = renderer == middle;
        end.enabled = renderer == end;
    }

    public void SetDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public void SetPaintColor(Color color)
    {
        paintColor = color;
        shouldPaint = true;
    }

    public void DestroyAfter(float seconds)
    {
        if (shouldPaint) Invoke(nameof(PaintTile), seconds * 0.7f);
        Destroy(gameObject, seconds);
    }

    private void PaintTile()
    {
        if (TilePainter.Instance != null) {
            TilePainter.Instance.Paint(transform.position, paintColor);
        }
    }
}

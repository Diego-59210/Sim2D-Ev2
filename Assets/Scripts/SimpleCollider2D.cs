using UnityEngine;

public enum ShapeType
{
    Rect,
    Circle
}

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleCollider2D : MonoBehaviour
{
    public ShapeType shapeType = ShapeType.Rect;
    public string collisionTag = "Default";
    public string ignoreTag = "";

    [HideInInspector] public Bounds bounds;
    [HideInInspector] public CircleBounds circleBounds;

    [Header("Debug")]
    public bool showGizmo = true;
    public Color gizmoColor = new Color(1, 0, 0, 0.3f);

    private SpriteRenderer spriteRenderer;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateBounds();
        CustomCollisionManager.RegisterCollider(this);
    }

    void OnDisable()
    {
        CustomCollisionManager.UnregisterCollider(this);
    }

    void LateUpdate() => UpdateBounds();

    public void UpdateBounds()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        bounds = spriteRenderer.bounds;

        if (shapeType == ShapeType.Circle)
        {
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            circleBounds = new CircleBounds(bounds.center, radius);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        UpdateBounds();
        Gizmos.color = gizmoColor;

        if (shapeType == ShapeType.Rect)
            Gizmos.DrawCube(bounds.center, bounds.size);
        else
            DrawCircleGizmo(circleBounds.center, circleBounds.radius);
    }

    private void DrawCircleGizmo(Vector2 center, float radius)
    {
        int segments = 32;
        Vector3 prev = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}

[System.Serializable]
public struct CircleBounds
{
    public Vector2 center;
    public float radius;
    public CircleBounds(Vector2 c, float r) { center = c; radius = r; }
}

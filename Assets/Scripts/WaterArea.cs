using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class WaterArea : MonoBehaviour
{
    [Header("Buoyancy")]

    public float buoyancyStrength = 15f;
    public float waterDrag = 0.9f;
    public float waterDensity = 1f;

    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = new Color(0f, 0.3f, 1f, 0.3f);

    private Bounds waterBounds;

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        waterBounds = sr.bounds;
        CustomCollisionManager.RegisterWater(this);
    }

    void OnDestroy()
    {
        CustomCollisionManager.UnregisterWater(this);
    }

    public void ApplyBuoyancy(IEnumerable<PhysicsBody2D> bodies)
    {
        foreach (var body in bodies)
        {
            if (body == null || !body.isActiveAndEnabled) continue;
            if (!body.TryGetComponent(out SimpleCollider2D col)) continue;

            Bounds b = col.bounds;

            // Saltar si no hay intersección con el agua
            if (!waterBounds.Intersects(b)) continue;

            // Calcular porcentaje sumergido (aproximado)
            float top = b.max.y;
            float bottom = b.min.y;
            float waterTop = waterBounds.max.y;
            float waterBottom = waterBounds.min.y;

            float submergedHeight = Mathf.Min(top, waterTop) - Mathf.Max(bottom, waterBottom);
            float submergedRatio = Mathf.Clamp01(submergedHeight / (b.size.y + 0.0001f));

            if (submergedRatio <= 0f) continue;

            // Calcular fuerza de empuje
            float buoyantForce = buoyancyStrength * submergedRatio * waterDensity;

            // Aplicar la fuerza hacia arriba
            body.AddForce(Vector2.up * buoyantForce * Time.deltaTime);

            // Aplicar amortiguación
            body.velocity *= Mathf.Lerp(1f, waterDrag, submergedRatio);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            waterBounds = sr.bounds;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(waterBounds.center, waterBounds.size);
    }
}
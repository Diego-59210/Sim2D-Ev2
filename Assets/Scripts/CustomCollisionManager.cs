using System.Collections.Generic;
using UnityEngine;

public class CustomCollisionManager : MonoBehaviour
{
    public static CustomCollisionManager instance;

    private readonly HashSet<SimpleCollider2D> colliders = new();
    private readonly HashSet<PhysicsBody2D> bodies = new();
    private readonly HashSet<WaterArea> waters = new();
    private readonly HashSet<HeatBody2D> heatBodies = new();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void FixedUpdate()
    {
        // Actualizar Bounds antes de la simulación
        foreach (var col in colliders)
        {
            if (col != null && col.isActiveAndEnabled)
                col.UpdateBounds();
        }

        // Simular cuerpos físicos
        float dt = Time.fixedDeltaTime;
        SimulateBodies(dt);

        // Resolver colisiones
        ResolveCollisions();

        // Aplicar fuerzas del agua (si existen)
        ApplyWaterForces();

        // Aplicar transferencia de calor
        ApplyHeatTransfer(dt);
    }

    void SimulateBodies(float dt)
    {
        foreach (var body in bodies)
        {
            if (body != null && body.isActiveAndEnabled)
                body.Simulate(dt);
        }
    }
    void ApplyWaterForces()
    {
        foreach (var w in waters)
            if (w != null) w.ApplyBuoyancy(bodies);
    }
    private void ApplyHeatTransfer(float deltaTime)
    {
        foreach (var a in heatBodies)
        {
            foreach (var b in heatBodies)
            {
                if (a == b) continue;

                float distance = Vector2.Distance(a.transform.position, b.transform.position);
                float maxRange = Mathf.Max(a.conductionRange, b.conductionRange);

                if (distance <= maxRange)
                {
                    // Cuanto más cerca estén, más intensa la transferencia
                    float distanceFactor = 1f - (distance / maxRange);
                    a.TransferHeat(b, deltaTime * distanceFactor);
                }
            }
        }
    }
    void ResolveCollisions()
    {
        foreach (var body in bodies)
        {
            if (!body.isActiveAndEnabled) continue;
            var colliderA = body.GetComponent<SimpleCollider2D>();
            if (!colliderA || colliderA.isTrigger) continue;

            foreach (var colliderB in colliders)
            {
                if (colliderA == colliderB) continue;
                if (colliderB.isTrigger) continue;

                if (!string.IsNullOrEmpty(colliderA.ignoreTag) && colliderB.CompareTag(colliderA.ignoreTag))
                    continue;

                if (CheckCollision(colliderA, colliderB, out Vector2 normal, out float penetration))
                {
                    body.ResolveCollision(normal, penetration);
                }
            }

            if (body.isGrounded && Mathf.Abs(body.velocity.y) < 0.05f)
                body.velocity.y = 0;
        }
    }



    bool CheckCollision(SimpleCollider2D a, SimpleCollider2D b, out Vector2 normal, out float penetration)
    {
        normal = Vector2.zero;
        penetration = 0f;

        if (a.shapeType == ShapeType.Circle && b.shapeType == ShapeType.Rect)
            return CircleRectCollision(a, b, out normal, out penetration);
        if (a.shapeType == ShapeType.Rect && b.shapeType == ShapeType.Rect)
            return RectRectCollision(a, b, out normal, out penetration);
        if (a.shapeType == ShapeType.Circle && b.shapeType == ShapeType.Circle)
            return CircleCircleCollision(a, b, out normal, out penetration);

        return false;
    }

    bool RectRectCollision(SimpleCollider2D a, SimpleCollider2D b, out Vector2 normal, out float penetration)
    {
        normal = Vector2.zero;
        penetration = 0f;

        Bounds A = a.bounds;
        Bounds B = b.bounds;

        if (!A.Intersects(B)) return false;

        float dx = (A.center.x < B.center.x) ? (A.max.x - B.min.x) : (B.max.x - A.min.x);
        float dy = (A.center.y < B.center.y) ? (A.max.y - B.min.y) : (B.max.y - A.min.y);

        if (dx < dy)
        {
            normal = (A.center.x < B.center.x) ? Vector2.left : Vector2.right;
            penetration = dx;
        }
        else
        {
            normal = (A.center.y < B.center.y) ? Vector2.down : Vector2.up;
            penetration = dy;
        }

        return true;
    }

    bool CircleRectCollision(SimpleCollider2D circle, SimpleCollider2D rect, out Vector2 normal, out float penetration)
    {
        normal = Vector2.zero;
        penetration = 0f;

        Bounds rb = rect.bounds;
        Vector2 circleCenter = (Vector2)circle.bounds.center;
        Vector2 closest = new Vector2(
            Mathf.Clamp(circleCenter.x, rb.min.x, rb.max.x),
            Mathf.Clamp(circleCenter.y, rb.min.y, rb.max.y)
        );

        Vector2 delta = circleCenter - closest;
        float dist = delta.magnitude;
        float radius = circle.bounds.extents.x;

        if (dist < radius)
        {
            normal = dist > 0 ? delta.normalized : Vector2.up;
            penetration = radius - dist;
            return true;
        }

        return false;
    }

    bool CircleCircleCollision(SimpleCollider2D a, SimpleCollider2D b, out Vector2 normal, out float penetration)
    {
        normal = Vector2.zero;
        penetration = 0f;

        Vector2 diff = (Vector2)a.bounds.center - (Vector2)b.bounds.center;
        float dist = diff.magnitude;
        float sumR = a.bounds.extents.x + b.bounds.extents.x;

        if (dist < sumR)
        {
            normal = diff.normalized;
            penetration = sumR - dist;
            return true;
        }

        return false;
    }
    public static bool CheckOverlap(SimpleCollider2D a, SimpleCollider2D b)
    {
        if (a.shapeType == ShapeType.Rect && b.shapeType == ShapeType.Rect)
            return a.bounds.Intersects(b.bounds);

        if (a.shapeType == ShapeType.Circle && b.shapeType == ShapeType.Circle)
            return Vector2.Distance(a.circleBounds.center, b.circleBounds.center) <=
                   (a.circleBounds.radius + b.circleBounds.radius);

        // Circle vs Rect
        if (a.shapeType == ShapeType.Circle && b.shapeType == ShapeType.Rect)
            return CircleRectOverlap(a.circleBounds, b.bounds);

        if (a.shapeType == ShapeType.Rect && b.shapeType == ShapeType.Circle)
            return CircleRectOverlap(b.circleBounds, a.bounds);

        return false;
    }

    private static bool CircleRectOverlap(CircleBounds circle, Bounds rect)
    {
        Vector2 closest = new(
            Mathf.Clamp(circle.center.x, rect.min.x, rect.max.x),
            Mathf.Clamp(circle.center.y, rect.min.y, rect.max.y)
        );

        float dist = Vector2.Distance(circle.center, closest);
        return dist < circle.radius;
    }

    public HashSet<SimpleCollider2D> GetAllColliders() => colliders;
    public HashSet<PhysicsBody2D> GetAllBodies() => bodies;

    public static void RegisterCollider(SimpleCollider2D col) => instance?.colliders.Add(col);
    public static void UnregisterCollider(SimpleCollider2D col) => instance?.colliders.Remove(col);
    public static void RegisterBody(PhysicsBody2D body) => instance?.bodies.Add(body);
    public static void UnregisterBody(PhysicsBody2D body) => instance?.bodies.Remove(body);
    public static void RegisterWater(WaterArea w) => instance?.waters.Add(w);
    public static void UnregisterWater(WaterArea w) => instance?.waters.Remove(w);
    public static void RegisterHeatBody(HeatBody2D h) => instance?.heatBodies.Add(h);
    public static void UnregisterHeatBody(HeatBody2D h) => instance?.heatBodies.Remove(h);
}

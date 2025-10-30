using System.Collections.Generic;
using UnityEngine;

public class CustomCollisionManager : MonoBehaviour
{
    public static CustomCollisionManager instance;

    private List<SimpleCollider2D> colliders = new List<SimpleCollider2D>();
    private List<PhysicsBody2D> bodies = new List<PhysicsBody2D>();
    private List<WaterArea> waters = new List<WaterArea>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        SimulateBodies(dt);
        ResolveCollisions();
        ApplyWaterForces();
    }

    void SimulateBodies(float dt)
    {
        foreach (var body in bodies)
        {
            if (!body.isActiveAndEnabled) continue;
            body.Simulate(dt);
        }
    }
    void ResolveCollisions()
    {
        foreach (var body in bodies)
        {
            if (!body.isActiveAndEnabled) continue;
            var colliderA = body.GetComponent<SimpleCollider2D>();
            if (!colliderA) continue;

            foreach (var colliderB in colliders)
            {
                if (colliderA == colliderB) continue;

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

    void ApplyWaterForces()
    {
        foreach (var w in waters)
            w.ApplyBuoyancy(bodies);
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
        Vector2 circleCenter = circle.bounds.center;
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

        Vector2 diff = a.bounds.center - b.bounds.center;
        float dist = diff.magnitude;
        float rA = a.bounds.extents.x;
        float rB = b.bounds.extents.x;
        float sumR = rA + rB;

        if (dist < sumR)
        {
            normal = diff.normalized;
            penetration = sumR - dist;
            return true;
        }

        return false;
    }

    // Métodos estáticos para registrar objetos físicos
    public static void RegisterCollider(SimpleCollider2D col) => instance?.colliders.Add(col);
    public static void UnregisterCollider(SimpleCollider2D col) => instance?.colliders.Remove(col);
    public static void RegisterBody(PhysicsBody2D body) => instance?.bodies.Add(body);
    public static void UnregisterBody(PhysicsBody2D body) => instance?.bodies.Remove(body);
    public static void RegisterWater(WaterArea w)=> instance?.waters.Add(w);
    public static void UnregisterWater(WaterArea w) => instance?.waters.Remove(w);

}

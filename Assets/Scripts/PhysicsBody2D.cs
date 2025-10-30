using UnityEngine;

[RequireComponent(typeof(SimpleCollider2D))]
public class PhysicsBody2D : MonoBehaviour
{
    public Vector2 velocity;
    public bool useGravity = true;
    public float gravity = -9.8f;
    public float bounciness = 0.8f;
    public float groundFriction = 0.8f;
    public bool isGrounded;

    private const float minBounceVelocity = 0.1f;
    private const float separationOffset = 0.001f;

    private SimpleCollider2D coll;
    private int groundContactCount = 0;

    void OnEnable()
    {
        coll = GetComponent<SimpleCollider2D>();
        CustomCollisionManager.RegisterBody(this);
    }

    void OnDisable()
    {
        CustomCollisionManager.UnregisterBody(this);
    }

    public void Simulate(float deltaTime)
    {
        if (useGravity && !isGrounded)
            velocity.y += gravity * deltaTime;

        transform.position += (Vector3)(velocity * deltaTime);

        isGrounded = false;
        groundContactCount = 0;
    }

    public void ResolveCollision(Vector2 normal, float penetration)
    {
        // Sólo aplicar corrección si hay penetración real
        if (penetration > 0)
            transform.position += (Vector3)(normal * (penetration + separationOffset));

        float vDotN = Vector2.Dot(velocity, normal);
        if (vDotN < 0)
        {
            Vector2 reflected = velocity - (1 + bounciness) * vDotN * normal;
            if (reflected.magnitude < minBounceVelocity)
                reflected = Vector2.zero;

            velocity = reflected;
        }

        // Si la colisión es principalmente vertical (suelo o techo)
        if (Mathf.Abs(normal.y) > 0.5f)
        {
            groundContactCount++;

            // Marcar grounded sólo si el normal apunta hacia arriba
            if (normal.y > 0)
                isGrounded = true;

            // Pequeño umbral para ignorar micro rebotes verticales
            if (Mathf.Abs(velocity.y) < 0.2f)
                velocity.y = 0;

            // Aplicar fricción sólo cuando está sobre el suelo
            if (isGrounded)
                velocity.x *= groundFriction;
        }
    }

    public void AddForce(Vector2 force)
    {
        velocity += force;
    }
}
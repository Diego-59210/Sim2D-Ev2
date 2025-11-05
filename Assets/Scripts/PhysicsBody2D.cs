using UnityEngine;

[RequireComponent(typeof(SimpleCollider2D))]
public class PhysicsBody2D : MonoBehaviour
{
    [Header("Movement")]
    public Vector2 velocity;
    public bool useGravity = true;
    public float gravity = -9.8f;

    [Header("Collsion")]
    public float bounciness = 0.8f;
    public float groundFriction = 0.8f;
    public bool isGrounded;

    [Header("Stability")]
    public float restThreshold = 0.05f; 
    public float minBounceVelocity = 0.1f;
    private const float separationOffset = 0.001f;
    private const float maxCorrection = 0.3f;
    private const int subSteps = 5;

    [Header("Debug")]
    public bool showDebug = true;
    public float debugScale = 0.5f;
    private Vector2 lastCollisionNormal = Vector2.zero;
    private SimpleCollider2D coll;

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
        isGrounded = false;

        float dt = deltaTime / subSteps;
        for (int i = 0; i < subSteps; i++)
        {
            if (useGravity && !isGrounded)
                velocity.y += gravity * dt;

            transform.position += (Vector3)(velocity * dt);
        }

        if (showDebug)
        {
            Vector3 velVec3 = (Vector3)velocity;
            Debug.DrawLine(transform.position, transform.position + velVec3 * debugScale, Color.yellow);
        }
    }

    private int restFrames = 0; 

    public void ResolveCollision(Vector2 normal, float penetration)
    {
        if (penetration <= 0f) return;

        // Corrige la posición 
        transform.position += (Vector3)(normal * Mathf.Min(penetration + separationOffset, maxCorrection));

        // Rebote solo si nos movemos hacia la superficie
        float vDotN = Vector2.Dot(velocity, normal);
        if (vDotN < 0f)
        {
            Vector2 reflected = velocity - (1 + bounciness) * vDotN * normal;

            if (reflected.magnitude < minBounceVelocity)
                reflected = Vector2.zero;

            velocity = reflected;
        }

        lastCollisionNormal = normal;

        // Si la colisión es con el suelo o techo
        if (Mathf.Abs(normal.y) > 0.5f)
        {
            if (normal.y > 0f)
                isGrounded = true;

            if (isGrounded)
            {
                // Frena horizontalmente
                velocity.x *= groundFriction;

                // Si la velocidad vertical es mínima, forzar reposo
                if (Mathf.Abs(velocity.y) < restThreshold)
                    velocity.y = 0f;

                // Detección acumulativa de reposo
                if (velocity.magnitude < restThreshold)
                {
                    restFrames++;
                    if (restFrames > 3)
                    {
                        velocity = Vector2.zero;
                        restFrames = 3; 
                    }
                }
                else
                {
                    restFrames = 0;
                }
            }
            else
            {
                restFrames = 0;
            }
        }

        //  Pérdida de energía sutil
        velocity *= 0.99f;
    }


    public void AddForce(Vector2 force)
    {
        velocity += force;
    }
    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // Color según estado
        Color stateColor = (!Application.isPlaying)
            ? Color.gray
            : (isGrounded && velocity.magnitude < restThreshold ? Color.red : Color.green);

        // Cuerpo
        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        // Vector de velocidad
        if (Application.isPlaying && velocity != Vector2.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(velocity * debugScale));
        }

        // Normal de la última colisión
        if (Application.isPlaying && lastCollisionNormal != Vector2.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(lastCollisionNormal * 0.4f));
        }
    }
}
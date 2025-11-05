using UnityEngine;

[RequireComponent(typeof(PhysicsBody2D))]
[RequireComponent(typeof(SimpleCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float jumpCooldown = 0.1f;

    [Header("Ground Check")]
    public float groundedCheckOffset = 0.05f;

    private PhysicsBody2D body;
    private SimpleCollider2D col;
    private bool canJump = true;

    void Awake()
    {
        body = GetComponent<PhysicsBody2D>();
        col = GetComponent<SimpleCollider2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");

        // Movimiento horizontal manual
        Vector2 velocity = body.velocity;
        velocity.x = inputX * moveSpeed;

        body.velocity = velocity;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && body.isGrounded && canJump)
        {
            canJump = false;
            body.velocity = new Vector2(body.velocity.x, jumpForce);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void ResetJump()
    {
        canJump = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundedCheckOffset);
    }
}
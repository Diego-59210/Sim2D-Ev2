using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleCollider2D))]
public class GravityZone2D : MonoBehaviour
{
    public enum GravityMode { Directional, Center }
    public enum GravityDirection { Up, Down, Left, Right }

    [Header("Gravity Settings")]
    public GravityMode mode = GravityMode.Directional;
    public GravityDirection direction = GravityDirection.Down;
    public float gravityConstant = 9.8f;
    public float scale = 50f;
    public bool resetOnExit = true;
    public string targetTag = "Player";

    [Header("Debug")]
    public bool showDebug = true;
    public Color gizmoColor = new Color(0.2f, 0.6f, 1f, 0.2f);

    private SimpleCollider2D zoneCollider;
    private HashSet<PhysicsBody2D> bodiesInside = new HashSet<PhysicsBody2D>();

    void Awake()
    {
        zoneCollider = GetComponent<SimpleCollider2D>();
        zoneCollider.isTrigger = true;
    }

    void Update()
    {
        var colliders = CustomCollisionManager.instance.GetAllColliders();
        HashSet<PhysicsBody2D> currentInside = new HashSet<PhysicsBody2D>();

        foreach (var col in colliders)
        {
            if (!col.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag)) continue;

            bool overlap = CustomCollisionManager.CheckOverlap(zoneCollider, col);
            if (!overlap) continue;

            var body = col.GetComponent<PhysicsBody2D>();
            if (body == null) continue;

            currentInside.Add(body);

            // Nuevo objeto entrando
            if (!bodiesInside.Contains(body))
            {
                body.useGravity = false;
            }

            // Aplicar gravedad cada frame
            ApplyGravity(body);
        }

        // Detectar salidas
        foreach (var body in bodiesInside)
        {
            if (!currentInside.Contains(body))
            {
                if (resetOnExit) body.useGravity = true;
            }
        }

        bodiesInside = currentInside;
    }

    private void ApplyGravity(PhysicsBody2D body)
    {
        if (body == null) return;

        Vector2 forceDir = Vector2.zero;

        if (mode == GravityMode.Directional)
        {
            switch (direction)
            {
                case GravityDirection.Up: forceDir = Vector2.up; break;
                case GravityDirection.Down: forceDir = Vector2.down; break;
                case GravityDirection.Left: forceDir = Vector2.left; break;
                case GravityDirection.Right: forceDir = Vector2.right; break;
            }
        }
        else 
        {
            Vector2 toCenter = (Vector2)transform.position - (Vector2)body.transform.position;
            forceDir = toCenter.normalized;
        }

        float distance = Vector2.Distance(body.transform.position, transform.position);
        float forceMagnitude = (mode == GravityMode.Center)
            ? gravityConstant * scale / Mathf.Max(distance * distance, 0.1f)
            : gravityConstant * scale;

        Vector2 force = forceDir * forceMagnitude * Time.deltaTime;
        body.AddForce(force);
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        Gizmos.color = gizmoColor;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            Gizmos.DrawCube(sr.bounds.center, sr.bounds.size);

        // Flecha direccional
        Vector3 dir = Vector3.zero;
        switch (direction)
        {
            case GravityDirection.Up: dir = Vector3.up; break;
            case GravityDirection.Down: dir = Vector3.down; break;
            case GravityDirection.Left: dir = Vector3.left; break;
            case GravityDirection.Right: dir = Vector3.right; break;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + dir * 1f);
    }
}
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleCollider2D))]
public class Spring2D : MonoBehaviour
{
    [Header("Spring Settings")]
    [Tooltip("Constante del resorte (k). Mayor -> más rígido")]
    public float springConstant = 120f;   // k (ajustar si es necesario)

    [Tooltip("Longitud en reposo del resorte")]
    public float restLength = 1.0f;

    [Tooltip("Amortiguación (c). A mayor -> menos oscilación")]
    public float damping = 2.0f;

    [Tooltip("Multiplicador global de fuerza (útil para proyectiles muy rápidos)")]
    public float forceMultiplier = 1.0f;

    [Tooltip("Si true, el otro extremo está fijo en transform.position")]
    public bool isAnchored = true;

    [Tooltip("Si no está anclado, este es el otro extremo (debe tener PhysicsBody2D)")]
    public Transform connectedBody;

    [Tooltip("Etiqueta de colisión que afectará el resorte")]
    public string targetTag = "Ball";

    [Header("Behavior")]
    public bool resetVelocityOnExit = false;

    [Header("Debug")]
    public bool showDebug = true;
    public Color gizmoColor = new Color(0.4f, 1f, 0.4f, 0.25f);
    public Color forceColor = Color.yellow;

    private SimpleCollider2D zoneCollider;
    private HashSet<PhysicsBody2D> bodiesInside = new HashSet<PhysicsBody2D>();

    void Awake()
    {
        zoneCollider = GetComponent<SimpleCollider2D>();
        zoneCollider.isTrigger = true;
    }

    void Update()
    {
        // Recopilar colliders dentro del manager y aplicar fuerza cada frame
        var all = CustomCollisionManager.instance.GetAllColliders();
        HashSet<PhysicsBody2D> current = new HashSet<PhysicsBody2D>();

        foreach (var col in all)
        {
            if (col == null) continue;
            if (!col.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag)) continue;

            if (!CustomCollisionManager.CheckOverlap(zoneCollider, col)) continue;

            var body = col.GetComponent<PhysicsBody2D>();
            if (body == null) continue;

            current.Add(body);
            ApplySpringForce(body);
        }

        // Detectar salidas
        foreach (var b in bodiesInside)
        {
            if (!current.Contains(b))
            {
                if (resetVelocityOnExit)
                    b.velocity = Vector2.zero;
            }
        }

        bodiesInside = current;
    }

    private void ApplySpringForce(PhysicsBody2D body)
    {
        if (body == null) return;

        // Punto de anclaje (si está anclado, es transform.position; si no, connectedBody.position)
        Vector2 anchor = isAnchored ? (Vector2)transform.position : (connectedBody ? (Vector2)connectedBody.position : (Vector2)transform.position);

        Vector2 bodyPos = body.transform.position;
        Vector2 toBody = bodyPos - anchor;
        float distance = toBody.magnitude;
        Vector2 dir = distance > 0.0001f ? toBody / distance : Vector2.up;

        // Desplazamiento respecto al restLength (positivo si estirado, negativo si comprimido)
        float displacement = distance - restLength;

        // Fuerza de Hooke (dirección hacia/desde el ancla)
        // Nota: usamos sign inverso para que la fuerza apunte hacia el punto de reposo
        Vector2 hookeForce = -springConstant * displacement * dir;

        // Si el cuerpo está más cerca del ancla que el reposo, invertir la dirección (empuje)
        if (displacement < 0f)
            hookeForce = -hookeForce;

        // Amortiguación: solo en la componente radial (largo del resorte)
        // Calculamos la velocidad relativa en la dirección radial
        Vector2 relVel = body.velocity;
        // Si connectedBody es dinámico, considerar su velocidad relativa
        if (!isAnchored && connectedBody != null)
        {
            var otherBody = connectedBody.GetComponent<PhysicsBody2D>();
            if (otherBody != null) relVel = body.velocity - otherBody.velocity;
        }
        float velAlongDir = Vector2.Dot(relVel, dir);
        Vector2 dampingForce = -damping * velAlongDir * dir;

        // Fuerza total (tratamos la fuerza resultante como aceleración)
        Vector2 totalAccel = (hookeForce + dampingForce) * forceMultiplier;

        // Convertimos a delta-vel: dv = a * dt (porque AddForce suma directamente a velocity)
        Vector2 dv = totalAccel * Time.deltaTime;

        // Aplicar a body
        body.AddForce(dv);

        // Si el otro extremo es dinámico, aplicar la reacción igual y opuesta
        if (!isAnchored && connectedBody != null)
        {
            var otherBody = connectedBody.GetComponent<PhysicsBody2D>();
            if (otherBody != null)
            {
                // reacción
                otherBody.AddForce(-dv);
            }
        }

        // Guardar en set para detectar salidas (opcional)
        bodiesInside.Add(body);
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;
        if (!zoneCollider) zoneCollider = GetComponent<SimpleCollider2D>();

        zoneCollider.UpdateBounds();
        Gizmos.color = gizmoColor;
        if (zoneCollider.shapeType == ShapeType.Rect)
            Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        else
            Gizmos.DrawWireSphere(zoneCollider.circleBounds.center, zoneCollider.circleBounds.radius);

        // Dibujo del ancla / conexión
        Gizmos.color = Color.cyan;
        Vector3 anchor = isAnchored ? transform.position : (connectedBody ? connectedBody.position : transform.position);
        Gizmos.DrawSphere(anchor, 0.05f);
    }
}
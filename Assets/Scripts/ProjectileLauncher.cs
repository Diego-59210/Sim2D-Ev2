using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float launchForce = 10f;

    [Header("References")]
    public Transform firePoint; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LaunchProjectile();
        }
    }

    void LaunchProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 direction = (mousePos - firePoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        PhysicsBody2D body = projectile.GetComponent<PhysicsBody2D>();
        if (body != null)
        {
            body.velocity = direction * launchForce;
        }
    }
}
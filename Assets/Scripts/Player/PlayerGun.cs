using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shoot Point")]
    [SerializeField] private Transform firePoint;

    [Header("Cooldown")]
    [SerializeField] private float fireRate = 0.3f;

    private float nextFireTime;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") &&
            Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireRate;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity);

        Projectile projectileScript =
            projectile.GetComponent<Projectile>();

        Vector2 direction =
            transform.localScale.x > 0
            ? Vector2.right
            : Vector2.left;

        projectileScript.SetDirection(direction);
    }
}
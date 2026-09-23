using UnityEngine;

/// <summary>
/// Projectile tiré par les ennemis (Gunner, Turret…).
/// Se déplace dans une direction donnée et inflige des dégâts au joueur.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private int damage = 1;

    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Touche le joueur
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // Variante via le composant Entity (si le tag n'est pas présent)
        PlayerHealth health = collision.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

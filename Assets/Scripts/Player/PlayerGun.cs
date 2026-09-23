using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tir du joueur (toujours disponible). Direction = orientation du sprite.
/// Peut être appelé via PlayerController.OnShoot ou directement OnShoot.
/// </summary>
public class PlayerGun : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("Transform enfant devant le canon. Si vide, un offset automatique est utilisé.")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float firePointOffsetX = 0.55f;

    [Header("Cadence")]
    [Tooltip("Délai minimum entre deux tirs (secondes).")]
    [SerializeField] private float fireRate = 0.35f;

    [Header("Feedback")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator animator;

    private float nextFireTime;
    private Collider2D playerCollider;
    private PlayerController playerController;
    private static readonly int AnimShoot = Animator.StringToHash("Attack");

    private void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
        playerController = GetComponent<PlayerController>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started) TryShoot();
    }

    public bool TryShoot()
    {
        if (projectilePrefab == null || Time.time < nextFireTime) return false;

        nextFireTime = Time.time + fireRate;

        bool facingRight = playerController == null || playerController.FacingRight;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector3 spawnPos = GetFirePosition(facingRight);

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
            projectileScript.SetDirection(direction);

        IgnorePlayerCollision(projectile);

        if (muzzleFlash != null) muzzleFlash.Play();
        if (animator != null) animator.SetTrigger("Attack");
        PlaySfx(shootClip);
        return true;
    }

    private Vector3 GetFirePosition(bool facingRight)
    {
        if (firePoint != null) return firePoint.position;
        float dir = facingRight ? 1f : -1f;
        return transform.position + new Vector3(firePointOffsetX * dir, 0.1f, 0f);
    }

    private void IgnorePlayerCollision(GameObject projectile)
    {
        if (playerCollider == null) return;
        Collider2D[] cols = projectile.GetComponents<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
            Physics2D.IgnoreCollision(playerCollider, cols[i], true);
    }

    private static void PlaySfx(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Tir du joueur (toujours disponible). Direction = orientation du sprite
/// ou direction vers l'ennemi le plus proche si Aim Assist est activé.
///
/// Intègre AccessibilityManager (aim assist + haptiques).
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

    [Header("Aim Assist (utilisé si AccessibilityManager.aimAssist = true)")]
    [Tooltip("Layer des ennemis (laisser Everything si tu n'as pas de layer dédié).")]
    [SerializeField] private LayerMask enemyLayer = ~0;
    [Tooltip("Angle max (degrés) par rapport à la direction regardée pour accepter une cible.")]
    [SerializeField] private float aimAssistMaxAngle = 75f;

    [Header("Feedback")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator animator;

    private float nextFireTime;
    private Collider2D playerCollider;
    private PlayerController playerController;
    private static readonly int AnimShoot = Animator.StringToHash("Shoot");

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

        // Aim assist : dévie le tir vers l'ennemi le plus proche si l'option est active
        if (IsAimAssistEnabled())
        {
            Vector2 assisted = GetAimAssistDirection(facingRight);
            if (assisted.sqrMagnitude > 0.01f)
                direction = assisted;
        }

        Vector3 spawnPos = GetFirePosition(facingRight);

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
            projectileScript.SetDirection(direction);

        IgnorePlayerCollision(projectile);

        if (muzzleFlash != null) muzzleFlash.Play();
        if (animator != null) animator.SetTrigger(AnimShoot);
        PlaySfx(shootClip);

        // Haptique accessibilité
        if (AccessibilityManager.Instance != null)
            AccessibilityManager.Instance.PlayHaptic(0.2f, 0.35f, 0.08f);

        return true;
    }

    #region Aim Assist

    private bool IsAimAssistEnabled()
    {
        return AccessibilityManager.Instance != null && AccessibilityManager.Instance.aimAssist;
    }

    private float GetAimAssistRadius()
    {
        if (AccessibilityManager.Instance != null)
            return AccessibilityManager.Instance.aimAssistRadius;
        return 6f;
    }

    /// <summary>
    /// Cherche l'ennemi le plus proche dans le rayon d'aim assist.
    /// Préfère les cibles dans la direction regardée (angle max configurable).
    /// Retourne Vector2.zero si aucune cible valide.
    /// </summary>
    private Vector2 GetAimAssistDirection(bool facingRight)
    {
        float radius = GetAimAssistRadius();
        Vector2 origin = transform.position;
        Vector2 facingDir = facingRight ? Vector2.right : Vector2.left;

        // 1) Essayer d'abord avec les colliders (rapide et précis)
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, enemyLayer);
        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i];
            if (col == null || col.transform == transform) continue;

            // On ne cible que les vrais ennemis
            if (!IsValidEnemy(col.gameObject)) continue;

            Vector2 toTarget = (Vector2)col.transform.position - origin;
            float distance = toTarget.magnitude;
            if (distance < 0.15f) continue;

            // Filtre par angle (évite de tirer derrière soi)
            float angle = Vector2.Angle(facingDir, toTarget);
            if (angle > aimAssistMaxAngle) continue;

            // Score = distance (plus proche = mieux). On peut pondérer par l'angle si besoin.
            float score = distance + angle * 0.02f;
            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = col.transform;
            }
        }

        // 2) Fallback : chercher tous les composants Enemy (si pas de layer correctement configuré)
        if (bestTarget == null)
        {
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy e = enemies[i];
                if (e == null || !e.IsAlive) continue;

                Vector2 toTarget = (Vector2)e.transform.position - origin;
                float distance = toTarget.magnitude;
                if (distance > radius || distance < 0.15f) continue;

                float angle = Vector2.Angle(facingDir, toTarget);
                if (angle > aimAssistMaxAngle) continue;

                float score = distance + angle * 0.02f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = e.transform;
                }
            }
        }

        if (bestTarget == null) return Vector2.zero;

        Vector2 dir = ((Vector2)bestTarget.position - origin).normalized;
        return dir;
    }

    private static bool IsValidEnemy(GameObject go)
    {
        if (go.CompareTag("Enemy")) return true;
        if (go.GetComponent<Enemy>() != null) return true;
        if (go.GetComponentInParent<Enemy>() != null) return true;
        return false;
    }

    #endregion

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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualise le rayon d'aim assist dans l'éditeur
        float radius = Application.isPlaying && AccessibilityManager.Instance != null
            ? AccessibilityManager.Instance.aimAssistRadius
            : 6f;

        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}

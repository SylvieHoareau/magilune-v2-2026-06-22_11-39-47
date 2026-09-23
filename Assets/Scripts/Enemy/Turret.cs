using UnityEngine;

/// <summary>
/// Tourelle fixe : ne patrouille pas, vise et tire sur le joueur dès qu'il est détecté.
/// Animator attendu : paramètres "IsAlert" (bool) et "Attack" (trigger).
/// États recommandés : Idle → Attack (trigger) → retour Idle.
/// </summary>
public class Turret : Enemy
{
    [Header("Tir")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Point de sortie du projectile. Si vide, un offset est calculé.")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float firePointOffsetX = 0.5f;
    [SerializeField] private float fireRate = 1.2f;

    [Header("Orientation")]
    [Tooltip("Si true, le scale.x est inversé pour regarder le joueur.")]
    [SerializeField] private bool flipToFacePlayer = true;

    [Header("Feedback")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private ParticleSystem muzzleFlash;

    private float nextFireTime;
    private float baseScaleX = 1f;
    private bool facingRight = true;

    private static readonly int AnimIsAlert = Animator.StringToHash("IsAlert");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    protected override void Awake()
    {
        base.Awake();
        if (animator == null) animator = GetComponent<Animator>();
        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (baseScaleX < 0.01f) baseScaleX = 1f;
    }

    /// <summary>La tourelle ne se déplace jamais.</summary>
    protected override void Patrol()
    {
        SetAlert(false);
    }

    protected override void OnPlayerDetected()
    {
        SetAlert(true);
        FacePlayer();
        TryShoot();
    }

    private void FacePlayer()
    {
        if (player == null || !flipToFacePlayer) return;

        bool shouldFaceRight = player.position.x >= transform.position.x;
        if (shouldFaceRight == facingRight) return;

        facingRight = shouldFaceRight;
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? baseScaleX : -baseScaleX;
        transform.localScale = scale;
    }

    private void TryShoot()
    {
        if (bulletPrefab == null || player == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + 1f / Mathf.Max(0.1f, fireRate);

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector3 spawnPos = GetFirePosition();

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        EnemyProjectile proj = bullet.GetComponent<EnemyProjectile>();
        if (proj != null)
            proj.SetDirection(direction);

        // Optionnel : orienter le sprite du projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (animator != null) animator.SetTrigger(AnimAttack);
        if (muzzleFlash != null) muzzleFlash.Play();
        PlaySfx(shootClip);
    }

    private Vector3 GetFirePosition()
    {
        if (firePoint != null) return firePoint.position;

        float dir = facingRight ? 1f : -1f;
        return transform.position + new Vector3(firePointOffsetX * dir, 0.1f, 0f);
    }

    private void SetAlert(bool alert)
    {
        if (animator != null) animator.SetBool(AnimIsAlert, alert);
    }

    private static void PlaySfx(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}

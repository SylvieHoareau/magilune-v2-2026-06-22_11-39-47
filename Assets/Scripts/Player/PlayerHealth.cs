using System.Collections;
using UnityEngine;

/// <summary>
/// Vies du joueur + dégâts Poison / Lave (consignes Magilune).
/// Ne détruit pas le GameObject : respawn au checkpoint pour un public 3+.
/// </summary>
public class PlayerHealth : Entity
{
    [Header("Invulnérabilité")]
    [SerializeField] private float iFrameDuration = 1.1f;

    [Header("Dégâts environnementaux")]
    [Tooltip("Dégâts par contact avec la lave (un tick par i-frame).")]
    [SerializeField] private int lavaDamage = 1;
    [Tooltip("Dégâts d'un tick de poison.")]
    [SerializeField] private int poisonDamage = 1;
    [SerializeField] private float poisonTickInterval = 0.75f;

    [Header("Feedback visuel / sonore (public jeune)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.45f, 0.7f, 1f);
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private Animator animator;

    [Header("Respawn")]
    [Tooltip("Laissé vide = position de départ de la scène.")]
    [SerializeField] private Transform respawnPoint;

    public event System.Action<int, int> HealthChanged;
    public event System.Action PlayerDied;
    public event System.Action PlayerRespawned;

    private bool isInvulnerable;
    private float poisonTimer;
    private int poisonOverlaps;
    private Vector3 startPosition;
    private Color defaultColor = Color.white;
    private static readonly int AnimHurt = Animator.StringToHash("Hurt");

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) defaultColor = spriteRenderer.color;
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public override void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0 || isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        PlayHitFeedback();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StopCoroutine(nameof(IFrameRoutine));
        StartCoroutine(IFrameRoutine());
    }

    protected override void Die()
    {
        PlayerDied?.Invoke();
        PlaySfx(deathClip);
        Respawn();
    }

    public void SetCheckpoint(Transform point)
    {
        if (point != null) respawnPoint = point;
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);

        Vector3 target = respawnPoint != null ? respawnPoint.position : startPosition;
        transform.position = target;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        poisonOverlaps = 0;
        poisonTimer = 0f;

        if (spriteRenderer != null) spriteRenderer.color = defaultColor;
        PlayerRespawned?.Invoke();

        StopAllCoroutines();
        StartCoroutine(IFrameRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPoison(other)) poisonOverlaps++;
        TryInstantHazard(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsLava(other)) TakeDamage(lavaDamage);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPoison(other)) poisonOverlaps = Mathf.Max(0, poisonOverlaps - 1);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsLava(collision.collider)) TakeDamage(lavaDamage);
    }

    private void Update()
    {
        if (poisonOverlaps <= 0 || currentHealth <= 0) return;

        poisonTimer += Time.deltaTime;
        if (poisonTimer >= poisonTickInterval)
        {
            poisonTimer = 0f;
            TakeDamage(poisonDamage);
        }
    }

    private void TryInstantHazard(Collider2D other)
    {
        if (IsLava(other)) TakeDamage(lavaDamage);
    }

    private static bool IsLava(Collider2D other)
    {
        return other.CompareTag("Lava") || other.CompareTag("Hazard");
    }

    private static bool IsPoison(Collider2D other)
    {
        return other.CompareTag("Poison");
    }

    private void PlayHitFeedback()
    {
        if (animator != null) animator.SetTrigger(AnimHurt);
        if (hitParticles != null) hitParticles.Play();
        PlaySfx(hitClip);
    }

    private IEnumerator IFrameRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        const float flashStep = 0.08f;

        while (elapsed < iFrameDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = spriteRenderer.color == defaultColor ? hitFlashColor : defaultColor;

            yield return new WaitForSeconds(flashStep);
            elapsed += flashStep;
        }

        if (spriteRenderer != null) spriteRenderer.color = defaultColor;
        isInvulnerable = false;
    }

    private static void PlaySfx(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }
}

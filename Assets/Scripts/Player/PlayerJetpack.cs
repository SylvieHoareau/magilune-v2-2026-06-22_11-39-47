using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Locomotion de la seconde partie du niveau (jetpack).
/// Interact maintenu = poussée. Carburant qui se recharge au sol.
/// Déverrouillé via AbilityZone, le pickup Jetpack, ou SetUnlocked(true).
///
/// Intègre AccessibilityManager (force, carburant max, régénération).
/// </summary>
public class PlayerJetpack : MonoBehaviour
{
    [Header("Déverrouillage")]
    [Tooltip("false en partie 1. Passe à true en partie 2.")]
    [SerializeField] private bool unlocked;

    [Header("Poussée")]
    [Tooltip("Force vers le haut. Avec Gravity Scale 3 et Mass 1, viser 35–45.")]
    [SerializeField] private float thrustForce = 42f;
    [SerializeField] private float maxRiseSpeed = 8f;

    [Header("Carburant")]
    [SerializeField] private float maxFuel = 3.5f;
    [SerializeField] private float fuelDrainPerSecond = 1f;
    [SerializeField] private float fuelRegenPerSecond = 1.4f;
    [SerializeField] private float minFuelToStart = 0.15f;

    [Header("Feedback")]
    [Tooltip("Enfant visuel (flamme) activé pendant la poussée.")]
    [SerializeField] private GameObject flameVisual;
    [SerializeField] private ParticleSystem thrustParticles;
    [SerializeField] private AudioClip thrustLoopClip;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private AudioSource thrustSource;
    private float currentFuel;
    private bool holdingThrust;
    private bool isThrusting;
    private static readonly int AnimJetpack = Animator.StringToHash("IsJetpacking");

    public bool IsUnlocked => unlocked;
    public bool IsThrusting => isThrusting;

    /// <summary>Carburant normalisé (0–1) en tenant compte du multiplicateur accessibilité.</summary>
    public float FuelNormalized
    {
        get
        {
            float effectiveMax = GetEffectiveMaxFuel();
            return effectiveMax <= 0f ? 0f : currentFuel / effectiveMax;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        currentFuel = GetEffectiveMaxFuel();
        if (animator == null) animator = GetComponent<Animator>();

        thrustSource = gameObject.AddComponent<AudioSource>();
        thrustSource.playOnAwake = false;
        thrustSource.loop = true;
        thrustSource.spatialBlend = 0f;
        thrustSource.volume = 0.45f;

        SetFlame(false);
    }

    public void SetUnlocked(bool enable)
    {
        unlocked = enable;
        if (!enable)
        {
            holdingThrust = false;
            StopThrustVisuals();
        }
        else
        {
            // On recharge un peu quand on débloque le jetpack
            currentFuel = Mathf.Max(currentFuel, GetEffectiveMaxFuel() * 0.5f);
        }
    }

    public void HandleInteract(InputAction.CallbackContext context)
    {
        if (!unlocked) return;

        if (context.performed) holdingThrust = true;
        else if (context.canceled) holdingThrust = false;
    }

    private void Update()
    {
        float effectiveMax = GetEffectiveMaxFuel();
        float effectiveRegen = GetEffectiveRegen();

        bool wantsThrust = unlocked && holdingThrust && currentFuel > 0f;
        if (wantsThrust && !isThrusting && currentFuel < minFuelToStart)
            wantsThrust = false;

        isThrusting = wantsThrust;

        if (isThrusting)
            currentFuel = Mathf.Max(0f, currentFuel - fuelDrainPerSecond * Time.deltaTime);
        else if (playerController != null && playerController.IsGrounded)
            currentFuel = Mathf.Min(effectiveMax, currentFuel + effectiveRegen * Time.deltaTime);

        // Clamp au cas où le multiplicateur change en runtime
        currentFuel = Mathf.Clamp(currentFuel, 0f, effectiveMax);

        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (!isThrusting || rb == null) return;

        float effectiveThrust = GetEffectiveThrust();
        rb.AddForce(Vector2.up * effectiveThrust, ForceMode2D.Force);

        Vector2 velocity = rb.linearVelocity;
        if (velocity.y > maxRiseSpeed)
        {
            velocity.y = maxRiseSpeed;
            rb.linearVelocity = velocity;
        }
    }

    #region Accessibilité – helpers

    private float GetEffectiveThrust()
    {
        if (AccessibilityManager.Instance != null)
            return AccessibilityManager.Instance.GetJetpackThrust(thrustForce);
        return thrustForce;
    }

    private float GetEffectiveMaxFuel()
    {
        if (AccessibilityManager.Instance != null)
            return AccessibilityManager.Instance.GetJetpackMaxFuel(maxFuel);
        return maxFuel;
    }

    private float GetEffectiveRegen()
    {
        if (AccessibilityManager.Instance != null)
            return AccessibilityManager.Instance.GetJetpackRegen(fuelRegenPerSecond);
        return fuelRegenPerSecond;
    }

    #endregion

    private void UpdateVisuals()
    {
        SetFlame(isThrusting);

        if (animator != null) animator.SetBool(AnimJetpack, isThrusting);

        if (isThrusting)
        {
            if (thrustParticles != null && !thrustParticles.isPlaying) thrustParticles.Play();
            if (thrustLoopClip != null && thrustSource != null && !thrustSource.isPlaying)
            {
                thrustSource.clip = thrustLoopClip;
                thrustSource.Play();
            }
        }
        else
        {
            StopThrustVisuals();
        }
    }

    private void StopThrustVisuals()
    {
        if (thrustParticles != null && thrustParticles.isPlaying) thrustParticles.Stop();
        if (thrustSource != null && thrustSource.isPlaying) thrustSource.Stop();
        SetFlame(false);
        if (animator != null) animator.SetBool(AnimJetpack, false);
        isThrusting = false;
    }

    private void SetFlame(bool on)
    {
        if (flameVisual != null && flameVisual.activeSelf != on)
            flameVisual.SetActive(on);
    }

    private void OnDisable() => StopThrustVisuals();
}

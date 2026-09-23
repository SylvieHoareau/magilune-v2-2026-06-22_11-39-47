using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Locomotion de la seconde partie du niveau (jetpack).
/// Interact maintenu = poussée. Carburant qui se recharge au sol.
/// Déverrouillé via AbilityZone, le pickup Jetpack, ou SetUnlocked(true).
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
    public float FuelNormalized => maxFuel <= 0f ? 0f : currentFuel / maxFuel;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        currentFuel = maxFuel;
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
    }

    public void HandleInteract(InputAction.CallbackContext context)
    {
        if (!unlocked) return;

        if (context.performed) holdingThrust = true;
        else if (context.canceled) holdingThrust = false;
    }

    private void Update()
    {
        bool wantsThrust = unlocked && holdingThrust && currentFuel > 0f;
        if (wantsThrust && !isThrusting && currentFuel < minFuelToStart)
            wantsThrust = false;

        isThrusting = wantsThrust;

        if (isThrusting)
            currentFuel = Mathf.Max(0f, currentFuel - fuelDrainPerSecond * Time.deltaTime);
        else if (playerController != null && playerController.IsGrounded)
            currentFuel = Mathf.Min(maxFuel, currentFuel + fuelRegenPerSecond * Time.deltaTime);

        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (!isThrusting || rb == null) return;

        rb.AddForce(Vector2.up * thrustForce, ForceMode2D.Force);

        Vector2 velocity = rb.linearVelocity;
        if (velocity.y > maxRiseSpeed)
        {
            velocity.y = maxRiseSpeed;
            rb.linearVelocity = velocity;
        }
    }

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

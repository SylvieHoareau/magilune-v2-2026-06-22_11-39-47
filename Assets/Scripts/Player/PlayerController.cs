using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Déplacement, saut (partie 1) et descente de plateforme (Bas + Interact).
/// Le jetpack est géré par <see cref="PlayerJetpack"/>, le tir par <see cref="PlayerGun"/>.
///
/// PlayerInput (Invoke Unity Events) :
///   Move     → OnMove
///   Jump     → OnJump
///   Interact → OnInteract
///   Shoot    → OnShoot
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    [Tooltip("Unités / seconde. 6–8 convient à un platformer enfant.")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Saut (première partie du niveau)")]
    [Tooltip("Vitesse verticale appliquée au saut. Avec Gravity Scale 3, 12–16 est un bon départ.")]
    [SerializeField] private float jumpForce = 14f;
    [Tooltip("Transform vide placé sous les pieds du joueur.")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.18f;
    [Tooltip("Layers Ground + PlatformPassThrough.")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("true en partie 1, false au milieu du niveau (trauma à la jambe).")]
    [SerializeField] private bool canJump = true;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Plateformes traversables")]
    [Tooltip("Layer des plateformes one-way (PlatformPassThrough).")]
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float dropThroughDuration = 0.35f;

    [Header("Feedback")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private ParticleSystem jumpDust;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerGun playerGun;
    private PlayerJetpack playerJetpack;
    private Vector2 moveInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool isGrounded;
    private bool facingRight = true;
    private float baseScaleX = 1f;
    private Coroutine dropRoutine;

    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimJump = Animator.StringToHash("Jump");

    public bool IsGrounded => isGrounded;
    public bool FacingRight => facingRight;
    public Vector2 MoveInput => moveInput;
    public bool CanJump => canJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerGun = GetComponent<PlayerGun>();
        playerJetpack = GetComponent<PlayerJetpack>();
        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (baseScaleX < 0.01f) baseScaleX = 1f;
    }

    private void Update()
    {
        CheckGrounded();

        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;

        if (canJump && jumpBufferCounter > 0f && coyoteCounter > 0f)
            PerformJump();

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        float x = moveInput.x * moveSpeed;
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
        UpdateFacing();
    }

    #region Input (New Input System — Invoke Unity Events)

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpBufferCounter = jumpBufferTime;

        // Relâcher le bouton coupe le saut (game feel Mario / Peach).
        if (context.canceled && rb.linearVelocity.y > 0f && canJump && (playerJetpack == null || !playerJetpack.IsThrusting))
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.45f);
    }

    /// <summary>
    /// Interact : Bas + Interact = descendre d'une plateforme.
    /// Sinon, active / relâche le jetpack (partie 2).
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context)
    {
        bool holdingDown = moveInput.y < -0.45f;

        if (context.started && holdingDown)
        {
            DropThroughPlatform();
            return;
        }

        playerJetpack?.HandleInteract(context);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            playerGun?.TryShoot();
            if (animator != null) animator.SetTrigger("Shoot");
        }
    }

    #endregion

    #region Saut & capacités

    private void PerformJump()
    {
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        isGrounded = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (animator != null) animator.SetTrigger(AnimJump);
        PlaySfx(jumpClip);
        if (jumpDust != null) jumpDust.Play();
    }

    public void SetJumpAbility(bool enable) => canJump = enable;

    public void SetJetpackAbility(bool enable)
    {
        if (playerJetpack != null)
            playerJetpack.SetUnlocked(enable);
    }

    #endregion

    #region Sol, flip, plateformes

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void UpdateFacing()
    {
        if (moveInput.x > 0.1f && !facingRight) Flip(true);
        else if (moveInput.x < -0.1f && facingRight) Flip(false);
    }

    private void Flip(bool right)
    {
        facingRight = right;
        Vector3 scale = transform.localScale;
        scale.x = (right ? 1f : -1f) * baseScaleX;
        transform.localScale = scale;
    }

    private void DropThroughPlatform()
    {
        if (dropRoutine != null) StopCoroutine(dropRoutine);
        dropRoutine = StartCoroutine(DropThroughRoutine());
    }

    private IEnumerator DropThroughRoutine()
    {
        Collider2D platform = groundCheck != null
            ? Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius + 0.08f, platformLayer)
            : null;

        if (platform != null)
        {
            PassThroughPlatform pass = platform.GetComponent<PassThroughPlatform>()
                                       ?? platform.GetComponentInParent<PassThroughPlatform>();
            if (pass != null)
                pass.DropThrough();
        }

        int playerLayer = gameObject.layer;
        int platformLayerIndex = LayerMask.NameToLayer("PlatformPassThrough");
        if (platformLayerIndex >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayerIndex, true);

        yield return new WaitForSeconds(dropThroughDuration);

        if (platformLayerIndex >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, platformLayerIndex, false);

        dropRoutine = null;
    }

    #endregion

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat(AnimSpeed, Mathf.Abs(moveInput.x));
        animator.SetBool(AnimGrounded, isGrounded);
    }

    private static void PlaySfx(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float moveSpeed = 8f;
    private float horizontalInput;

    [Header("Saut (Première partie)")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool canJump = true;
    private bool isGrounded;

    [Header("Jetpack (Seconde partie)")]
    [SerializeField] private bool hasJetpack = false;
    [SerializeField] private float jetpackForce = 15f;
    [SerializeField] private float maxJetpackSpeed = 10f;
    private bool isUsingJetpack;

    [Header("Plateformes Traversables")]
    [SerializeField] private LayerMask platformLayer;
    private Collider2D playerCollider;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Vérification si le joueur touche le sol
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        }
    }

    private void FixedUpdate()
    {
        // Déplacement horizontal
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Mouvement du Jetpack
        if (hasJetpack && isUsingJetpack)
        {
            ApplyJetpackForce();
        }
    }

    #region Input Handlers (New Input System)

    // Appelé par l'action 'Move' de l'Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        horizontalInput = input.x;

        // Détection de la combinaison : Bas + Interaction pour descendre d'une plateforme
        if (input.y < -0.5f && context.started)
        {
            StartCoroutine(DisablePlatformCollision());
        }
    }

    // Appelé par l'action 'Jump' de l'Input System
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && canJump && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Appelé par l'action 'Jetpack' ou 'Interact' de l'Input System
    public void OnJetpack(InputAction.CallbackContext context)
    {
        if (!hasJetpack) return;

        if (context.performed)
        {
            isUsingJetpack = true;
        }
        else if (context.canceled)
        {
            isUsingJetpack = false;
        }
    }

    #endregion

    #region Jetpack & Mechanics

    private void ApplyJetpackForce()
    {
        // Force constante vers le haut
        rb.AddForce(Vector2.up * jetpackForce, ForceMode2D.Force);

        // Limitation de la vitesse maximale en montée
        if (rb.linearVelocity.y > maxJetpackSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJetpackSpeed);
        }
    }

    // Permet d'activer/désactiver les capacités lors des événements du jeu
    public void SetJumpAbility(bool enable) => canJump = enable;
    public void SetJetpackAbility(bool enable) => hasJetpack = enable;

    // Traverser les plateformes vers le bas
    private IEnumerator DisablePlatformCollision()
    {
        // Désactive temporairement la collision entre le joueur et les plateformes traversables
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("PlatformPassThrough"), true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("PlatformPassThrough"), false);
    }

    #endregion
}

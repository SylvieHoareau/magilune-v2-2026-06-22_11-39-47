using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerController : Entity
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Abilities")]
    [SerializeField] private bool canJump = true;      // Désactivé au milieu
    [SerializeField] private bool hasLocomotion = false; // Grapple/Jetpack

    private Rigidbody2D rb;
    private bool isGrounded;

    // Branché sur le nouveau Input System (Gamepad)
    private @PlayerInput playerInputAsset;
    private InputAction moveAction;
    private InputAction jumpAction;
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

        playerInputAsset = new @PlayerInput();
        moveAction = playerInputAsset.Player.Move;
        jumpAction = playerInputAsset.Player.Jump;
    }

     private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // Mouvement
        float x = moveAction.ReadValue<Vector2>().x;
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        // Saut
        if (jumpAction.WasPressedThisFrame() && canJump && isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    private void OnShoot() { /* Instancie un projectile */ }

    // Appelé par le LevelManager au milieu du niveau
    public void DisableJump() => canJump = false;
    public void EnableLocomotion() => hasLocomotion = true;
}
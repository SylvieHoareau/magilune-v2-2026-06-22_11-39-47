using UnityEngine;

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
    private PlayerInput input;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
    }

    private void OnMove(InputValue value)
    {
        float x = value.Get<Vector2>().x;
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y);
    }

    private void OnJump()
    {
        if (canJump && isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnShoot() { /* Instancie un projectile */ }

    // Appelé par le LevelManager au milieu du niveau
    public void DisableJump() => canJump = false;
    public void EnableLocomotion() => hasLocomotion = true;
}
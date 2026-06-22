using UnityEngine;
using System.Collections;
using System;

namespace VerdantHeart
{
    public class PlayerStateMachine : StateMachine
    {
        [Header("Movement")]
        [SerializeField, Range(0f, 100f)] float maxSpeed = 5f; // max movement speed
        [SerializeField, Range(0f, 100f)] float maxAcceleration = 85f; // max movement acceleration on the ground. Higher number = tighter turning
        [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 55f; // max movement acceleration in the air. Higher number = tighter turning

        bool iCanMove = false;
        Vector2 moveDirection;
        Vector2 desiredVelocity;
        Vector2 velocity;
        float deadzone = 0.05f;
        float maxSpeedChange;
        float acceleration;

        [Header("Jumping")]
        [SerializeField, Range(0f, 10f)] float jumpHeight = 2.5f; // the max height of the jump
        [SerializeField, Range(0, 5)] int maxAirJumps = 1; // how many in-air jumps can the player make
        [SerializeField, Range(0f, 5f)] float climbGravityScale = 1f; // gravity scale going up
        [SerializeField, Range(0f, 5f)] float fallGravityScale = 4f; // gravity scale going down
        [SerializeField, Range(0, 100)] int jumpBuffer = 40; // amount of frames allowed before hitting the ground to register jump
        [SerializeField, Range(0, 100)] int coyoteTimeBuffer = 20;

        int jumpPhase;
        float defaultGravityScale = 1f;
        bool wantToJump = false;
        bool isOnCoyoteTime = false;
        bool isPlayerLanding = false;

        Ground ground;
        bool isGrounded = false;

        // Player Input
        bool endJumpAction = false;

        // setup component variables
        Animator animator;
        Rigidbody2D rb;
        [SerializeField] SpriteRenderer sr;

        #region GETTERS
        // Movement
        public Vector2 MoveDirection => moveDirection;
        public float Deadzone => deadzone;
        public bool WantToJump => wantToJump;
        public bool IsGrounded => isGrounded;
        public bool IsPlayerLanding => isPlayerLanding;

        // Components
        public Animator Animator => animator;
        public Rigidbody2D Rb => rb;
        #endregion

        void Awake()
        {
            // initialize component variables
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            ground = GetComponent<Ground>();
        }

        private void OnEnable()
        {
            Ground.PlayerLanded += PlayerHasLanded;
            Ground.PlayerLeftGround += StartCoyoteTime;
        }

        private void OnDisable()
        {
            Ground.PlayerLanded -= PlayerHasLanded;
            Ground.PlayerLeftGround -= StartCoyoteTime;
        }

        // start with the idle state
        private void Start() => SwitchState(new PlayerIdleState(this));

        public override void Update()
        {
            base.Update(); // get the Update method from StateMachine

            // reset endJumpAction if the player starts to fall or is grounded.
            if (rb.linearVelocityY < 0f || isGrounded)
                endJumpAction = false;

            // get data on player input for input variables
            ReadPlayerInput();

            // collect moveDirection data as it is needed for multiple states
            moveDirection.x = Input.GetAxis("Horizontal");
            // calculate the desired velocity, that will always be positive
            desiredVelocity = new Vector2(moveDirection.x, 0f) * Mathf.Max(maxSpeed - ground.Friction, 0);

            if (iCanMove)
            {
                // flip the sprite to face the proper direction
                HandleSpriteFacingDirection();
            }
        }

        private void FixedUpdate()
        {
            // check if grounded
            if (isOnCoyoteTime == false)
                isGrounded = ground.OnGround;

            // store current rigidbody velocity
            velocity = rb.linearVelocity;

            HandleMovement();

            // reset jumpPhases if grounded
            if (isGrounded)
                jumpPhase = 0;

            // if wantToJump, check if grounded, if so, jump, else, check a few frames if grounded, then jump if becomes true
            if (wantToJump)
            {
                wantToJump = false;// reset this to false

                if (isGrounded || jumpPhase < maxAirJumps)
                    Jump();

                if (isGrounded == false)
                    StartCoroutine(CheckJumpBuffer());
            }

            // while in air, calculate gravity scale
            HandleGravityScale();

            // if the jump button is let go, make upwards velocity = 0
            if (endJumpAction && rb.linearVelocityY >= 0f)
            {
                endJumpAction = false;
                velocity.y = 0f;
            }

            // apply all velocities after calculation
            rb.linearVelocity = velocity;
        }

        void PlayerHasLanded()
        {
            if (Mathf.Abs(moveDirection.x) > 0)
                SwitchState(new PlayerMoveState(this));
            else
                SwitchState(new PlayerIdleState(this));
        }

        public void SetPlayerLandingOn() => isPlayerLanding = true;
        public void ResetPlayerLanding() => isPlayerLanding = false;

        // INPUT METHODS
        private void ReadPlayerInput()
        {
            // Jump Actions from ReWired
            wantToJump |= Input.GetButtonDown("Jump"); // sets wantToJump to true and keeps true until we manually set to false

            endJumpAction |= Input.GetButtonUp("Jump"); // getting end of jump for leaving jump method early
        }

        #region MOVEMENT
        public void SetICanMove(bool value) => iCanMove = value;

        private void HandleSpriteFacingDirection()
        {
            // flip the direction the sprite is facing depending on left or right is pressed.
            if (moveDirection.x < 0)
                sr.flipX = true;
            if (moveDirection.x > 0)
                sr.flipX = false;
        }

        private void HandleMovement()
        {
            // set acceleration depending on in air or grounded
            acceleration = isGrounded ? maxAcceleration : maxAirAcceleration;

            // make acceleration time dependant
            maxSpeedChange = acceleration * Time.deltaTime;

            // take current velocity and approace desiredVelocity
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        }
        #endregion

        #region JUMPING
        private void Jump()
        {
            // switch state to jump state
            SwitchState(new PlayerJumpState(this));

            // increase the jumpPhase
            jumpPhase++;

            velocity.y = 0f;

            // calculate the vertical jump speed using formula Sqrt of 2g*jumpHeight. Using a -ve since upwards is -ve direction of gravity
            float jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * jumpHeight);

            // slowly decrease jump speed vertical movement over time, making sure the value is always positive
            if (velocity.y > 0f)
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);

            // add the jumpSpeed to the upwards velocity
            velocity.y += jumpSpeed;
        }

        private void HandleGravityScale()
        {
            if (rb.linearVelocityY > 0) // are we moving up?
                rb.gravityScale = climbGravityScale;
            else if (rb.linearVelocityY < 0)
                rb.gravityScale = fallGravityScale;
            else if (rb.linearVelocityY == 0)
                rb.gravityScale = defaultGravityScale;
        }

        IEnumerator CheckJumpBuffer()
        {
            wantToJump = true;

            // pause the wantToJump value by jumpBuffer
            for (int i = 0; i < jumpBuffer; i++)
            {
                yield return null;
            }
            wantToJump = false;
        }

        private void StartCoyoteTime()
        {
            isOnCoyoteTime = true;
            // use a coroutine to check the next few frames based on coyoteTimeBuffer variable
            StartCoroutine(CoyoteTime());
        }
        IEnumerator CoyoteTime()
        {
            isGrounded = true;

            // during the coyoteTimeBuffer period, keep isGrounded = true
            for (int i = 0; i < coyoteTimeBuffer; i++)
                yield return null;

            // toggle off coyoteTime so the real isGrounded can be implimented
            isOnCoyoteTime = false;
        }
        #endregion
    }
}
using UnityEngine;

namespace VerdantHeart
{
    public class PlayerIdleState : PlayerBaseState
    {
        CountdownTimer playerLandingTimer;
        float landingTime = 4f / 30f;
        public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {

            //Debug.Log("Entering Idle State");

            // reset the velocity of the player to zero at the start of this state.
            StateMachine.Rb.linearVelocity = Vector2.zero;

            // play the idle animation
            if (StateMachine.IsPlayerLanding == true)
            {
                playerLandingTimer = new CountdownTimer(landingTime);
                playerLandingTimer.OnTimerStop += PlayIdleAnim;
                playerLandingTimer.Start();
            }
            else
                PlayIdleAnim();
        }

        public override void Tick(float deltaTime)
        {
            if (playerLandingTimer != null)
                playerLandingTimer.Tick(deltaTime);

            HandleStateSwitching();
        }

        public override void Exit() { }

        void PlayIdleAnim()
        {
            StateMachine.ResetPlayerLanding();
            StateMachine.Animator.Play("idle");
        }

        private void HandleStateSwitching()
        {
            if (StateMachine.IsGrounded && Mathf.Abs(StateMachine.MoveDirection.x) > StateMachine.Deadzone)
                StateMachine.SwitchState(new PlayerMoveState(StateMachine));
        }
    }
}
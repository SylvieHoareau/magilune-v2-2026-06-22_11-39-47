using UnityEngine;

namespace VerdantHeart
{
    public class PlayerMoveState : PlayerBaseState
    {
        CountdownTimer playerLandingTimer;
        float landingTime = 4f / 30f;

        public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            //Debug.Log("Entering Move State");
            // Play the movement animation
            if (StateMachine.IsPlayerLanding == true)
            {
                playerLandingTimer = new CountdownTimer(landingTime);
                playerLandingTimer.OnTimerStop += PlayMoveAnim;
                playerLandingTimer.Start();
            }
            else
                PlayMoveAnim();

            StateMachine.SetICanMove(true);
        }

        public override void Tick(float deltaTime)
        {
            if (playerLandingTimer != null)
                playerLandingTimer.Tick(deltaTime);

            HandleStateSwitching();
        }

        public override void Exit() => StateMachine.SetICanMove(false);

        void PlayMoveAnim()
        {
            StateMachine.ResetPlayerLanding();
            StateMachine.Animator.Play("move");
        }

        private void HandleStateSwitching()
        {
            // switch to idle state if no longer moving on the ground
            if (StateMachine.IsGrounded && Mathf.Abs(StateMachine.MoveDirection.x) < StateMachine.Deadzone)
                StateMachine.SwitchState(new PlayerIdleState(StateMachine));
        }
    }
}
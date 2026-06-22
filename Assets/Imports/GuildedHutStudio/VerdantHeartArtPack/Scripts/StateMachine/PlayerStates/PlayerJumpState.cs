namespace VerdantHeart
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            //StateMachine.AudioDirector.PlayAudio(StateMachine.AudioDirector.AudioClipData[0]);
            StateMachine.Animator.Play("jumpStart");

            StateMachine.SetICanMove(true); // set bool to true so player can move side to side
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            //reset bools
            StateMachine.SetICanMove(false);
            StateMachine.Animator.Play("jumpEnd");
            StateMachine.SetPlayerLandingOn();
        }
    }
}
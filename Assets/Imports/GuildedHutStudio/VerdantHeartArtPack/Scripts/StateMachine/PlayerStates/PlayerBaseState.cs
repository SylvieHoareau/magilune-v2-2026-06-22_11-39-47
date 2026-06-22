namespace VerdantHeart
{
    public abstract class PlayerBaseState : State
    {
        protected PlayerStateMachine StateMachine;

        public PlayerBaseState(PlayerStateMachine stateMachine) => this.StateMachine = stateMachine;
    }
}
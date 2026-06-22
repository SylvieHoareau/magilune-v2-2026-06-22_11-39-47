using UnityEngine;

namespace VerdantHeart
{
    public abstract class StateMachine : MonoBehaviour
    {
        State _currentState;

        public virtual void Update() => _currentState?.Tick(Time.deltaTime);

        public void SwitchState(State newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }
    }
}
using UnityEngine;

namespace FSM
{
    public class FiniteStateMachine
    {
        private State currentState;

        public void Initialize(State startingState)
        {
            currentState = startingState;
            currentState?.Enter(this);
        }

        public bool ChangeState(State newState)
        {
            if (!currentState.ValidateTransition(newState))
            {
                Debug.Log($"Can't transition from state {currentState} to {newState}");
                return false;
            }

            currentState?.Exit();
            currentState = newState;
            currentState?.Enter(this);
            return true;
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (currentState == null)
            {
                Debug.LogWarning("State not initialized, but update was called");
            }
#endif
            currentState?.Execute();
        }
    }
}

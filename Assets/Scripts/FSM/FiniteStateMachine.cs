using UnityEngine;

namespace FSM
{
    public class FiniteStateMachine<T> where T : IBlackboard

    {
    private State<T> currentState;

    public void Initialize(State<T> startingState)
    {
        currentState = startingState;
        currentState?.Enter(this);
    }

    public bool ChangeState(State<T> newState)
    {
        if (!currentState.ValidateTransition(newState))
        {
            Debug.Log($"Can't transition from state {currentState.GetType()} to {newState.GetType()}");
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

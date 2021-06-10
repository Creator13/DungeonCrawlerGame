namespace FSM
{
    public class State<T> where T : IBlackboard

    {
    protected readonly T blackboard;
    protected FiniteStateMachine<T> parent;

    public State(T blackboard)
    {
        this.blackboard = blackboard;
    }

    public virtual void Enter(FiniteStateMachine<T> parent)
    {
        this.parent = parent;
    }

    public virtual void Execute() { }

    public virtual void Exit() { }

    public virtual bool ValidateTransition(State<T> newState)
    {
        return true;
    }
    }
}

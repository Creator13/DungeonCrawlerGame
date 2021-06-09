namespace FSM
{
    public class State
    {
        // protected readonly T blackboard;
        protected FiniteStateMachine parent;

        public State()
        {
            // this.blackboard = blackboard;
        }

        public virtual void Enter(FiniteStateMachine parent)
        {
            this.parent = parent;
        }

        public virtual void Execute() { }

        public virtual void Exit() { }

        public virtual bool ValidateTransition(State newState)
        {
            return true;
        }
    }
}

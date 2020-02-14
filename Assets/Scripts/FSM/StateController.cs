using UnityEngine;

namespace Astar
{
    public class StateController : MonoBehaviour
    {
        [SerializeField] private State currentState;
        private State remainState;

        private bool aiActive = true;
        private float stateTimeElapsed = 0;

        private Entity _entity;
        public Entity entity => _entity;

        public void Init(Entity entity)
        {
            currentState = Resources.Load<State>("FSM/States/IdleState");
            remainState = Resources.Load<State>("FSM/States/RemainState");
            _entity = entity;
        }

        public void Tick()
        {
            if (aiActive)
            {
                currentState.UpdateState(this); 
            }
        }

        public void TransitionToState(State nextState)
        {
            if (nextState != remainState)
            {
                currentState = nextState;
                OnExitState();
            }
        }

        public bool CheckIfCountDownElapsed(float duration)
        {
            stateTimeElapsed += Time.deltaTime;
            return (stateTimeElapsed >= duration);
        }

        private void OnExitState()
        {
            stateTimeElapsed = 0;
        }

        public void SetAIActive(bool value)
        {
            aiActive = value;
        }
    }
}
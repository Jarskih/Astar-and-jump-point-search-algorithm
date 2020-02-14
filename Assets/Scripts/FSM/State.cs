using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/State")]
    public class State : ScriptableObject
    {
        public Action[] actions;
        public Transition[] transitions;

        public void UpdateState(StateController controller)
        {
            DoActions(controller);
            CheckTransitions(controller);
        }

        private void DoActions(StateController controller)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Act(controller);
            }
        }

        private void CheckTransitions(StateController controller)
        {
            foreach (var t in transitions)
            {
                bool decisionSucceeded = t.decision.Decide(controller);

                controller.TransitionToState(decisionSucceeded ? t.trueState : t.falseState);
            }
        }
    }
}
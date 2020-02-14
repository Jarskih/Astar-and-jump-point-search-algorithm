using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Action/RestAction")]
    public class RestAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.entity.Rest();
        }
    }
}
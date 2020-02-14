using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Action/DropAction")]
    public class DropAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.entity.Drop();
        }
    }
}
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Decision/DropDecision")]
    public class DropDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.entity.CanDropItem();
        }
    }
}
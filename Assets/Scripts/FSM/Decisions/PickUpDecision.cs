using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Decision/PickUpDecision")]
    public class PickUpDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.entity.CanPickup();
        }
    }
}
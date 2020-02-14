using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Decision/MoveDecision")]
    public class MoveDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.entity.HasPath();
        }
    }
}

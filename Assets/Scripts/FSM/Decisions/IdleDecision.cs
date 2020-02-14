using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Decision/IdleDecision")]
    public class IdleDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.entity.HasPath() == false;
        }
    }
}

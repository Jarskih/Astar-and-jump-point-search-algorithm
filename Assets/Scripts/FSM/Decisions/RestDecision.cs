using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Decision/RestDecision")]
    public class RestDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.entity.NeedsRest();
        }
    }
}
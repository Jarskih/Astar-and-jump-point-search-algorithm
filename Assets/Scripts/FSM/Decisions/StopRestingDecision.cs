using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Decision/StopRestingDecision")]
    public class StopRestingDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.entity.DoneResting();
        }
    }
}
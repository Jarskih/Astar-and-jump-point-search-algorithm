using System.Collections;
using System.Collections.Generic;
using Astar;
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Action/IdleAction")]
    public class IdleAction : Action
    {
        public override void Act(StateController controller)
        {
            // Decide what to do and find path
            controller.entity.Decide();
        }
    }
}

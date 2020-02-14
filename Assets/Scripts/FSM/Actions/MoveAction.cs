using System;
using UnityEngine;

namespace Astar
{
    [CreateAssetMenu(menuName = "FSM/Action/MoveAction")]
    public class MoveAction : Action
    {
        public override void Act(StateController controller)
        {
            controller.entity.MoveAlongPath();
        }
    }
}
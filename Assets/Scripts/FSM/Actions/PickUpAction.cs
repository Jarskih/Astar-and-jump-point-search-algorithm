using UnityEngine;
 
 namespace Astar
 {
     [CreateAssetMenu(menuName = "FSM/Action/PickUpAction")]
     public class PickUpAction : Action
     {
         public override void Act(StateController controller)
         {
            controller.entity.Pickup();
         }
     }
 }
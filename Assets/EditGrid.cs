using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    public class EditGrid : MonoBehaviour
    {
        private Grid _grid;

        public void Init(Grid grid)
        {
            _grid = grid;
        }

        public void Tick()
        {
            if (Input.GetMouseButton(0))
            {
                EditObstacle(true);
            }

            if (Input.GetMouseButton(1))
            {
                EditObstacle(false);
            }
        }

        void EditObstacle(bool walkable)
        {
            var node = NodeHelpers.FindNodeFromMousePosition(ref _grid);
            node.SetWalkable(walkable);
        }
    }
}
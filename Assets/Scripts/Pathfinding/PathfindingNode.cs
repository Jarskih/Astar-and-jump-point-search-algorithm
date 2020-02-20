using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {

    public class PathfindingNode
{
        public PathDirections[] dirs = new PathDirections[4];

        private int _x, _y, _z;
        public int x => _x;
        public int y => _y;
        public int z => _z;

        // Pathfinding
        public float gCost;
        public int hCost;
        public float fCost => gCost + hCost;
        public PathfindingNode parentNode;
        public int distance;
        private bool _isWalkable;
        public bool isWalkable => _isWalkable;

        public PathfindingNode(int pX, int pY, int pZ, bool isWalkable)
        {
            _x = pX;
            _y = pY;
            _z = pZ;
            _isWalkable = isWalkable;

            for (int i = 0; i < 4; i++)
            {
                PathDirections pd = new PathDirections {d = (Direction) i, status = DirectionStatus.walkable};
                dirs[i] = pd;
            }
        }
        public PathDirections ReturnDirection(Direction d)
        {
            PathDirections retVal = null;

            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].d == d)
                {
                    retVal = dirs[i];
                    break;
                }
            }

            return retVal;
        }

        public void SetDirection(Direction d, DirectionStatus s)
        {
            PathDirections pd = ReturnDirection(d);
            pd.status = s;
        }

        public class PathDirections
        {
            public Direction d;
            public DirectionStatus status;
        }

        public enum DirectionStatus
        {
            walkable,
            partial,
            blocked
        }

        public enum Direction
        {
            n,
            s,
            w,
            e
        }
}
}

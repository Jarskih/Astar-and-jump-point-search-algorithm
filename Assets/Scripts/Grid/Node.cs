﻿using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    [System.Serializable]
    public class Node
    {
        private Entity _entity;
        public PathDirections[] dirs = new PathDirections[4];

        private int _x, _y, _z;
        public int x => _x;
        public int y => _y;
        public int z => _z;
        [SerializeField] private GameObject _nodeObject;

        // Pathfinding
        public float gCost;
        public int hCost;
        public int fCost;
        public Node parentNode;
        public int distance;
        [SerializeField] private bool _isWalkable;
        public bool isWalkable => _isWalkable;

        public Node(int pX, int pY, int pZ, GameObject go, bool isWalkable)
        {
            _x = pX;
            _y = pY;
            _z = pZ;
            _nodeObject = go;
            _isWalkable = isWalkable;

            for (int i = 0; i < 4; i++)
            {
                PathDirections pd = new PathDirections {d = (Direction) i, status = DirectionStatus.walkable};
                dirs[i] = pd;
            }
        }

        public Vector3Int GetNodeGridPos()
        {
            return new Vector3Int(_x, _y, _z);
        }

        public Vector3 GetNodeWorldPos()
        {
            return _nodeObject.transform.position;
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

        public bool HasEntity(Entity.EntityType entityType)
        {
            return _entity.GetEntityType() == entityType;
        }

        public void AddEntity(Entity entity)
        {
            _entity = entity;
        }

        public void RemoveEntity()
        {
            _entity = null;
        }

        public Entity GetEntity()
        {
            return _entity;
        }
    }
}
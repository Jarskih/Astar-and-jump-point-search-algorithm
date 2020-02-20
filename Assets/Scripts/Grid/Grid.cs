using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Astar
{
    [System.Serializable]
    public class Grid
    {
        // Visual representation of grid tile
       [SerializeField] private Node[,,] _nodes;
        private int _gridSizeX;
        private int _gridSizeY;
        private int _gridSizeZ;

        private int _nodeSizeX = 1;
        private int _nodeSizeY = 1;
        private int _nodeSizeZ = 1;
        public int sizeX => _gridSizeX;
        public int sizeY => _gridSizeY;
        public int sizeZ => _gridSizeZ;

        public bool Init(int pX, int pY, int pZ)
        {
            if (pX <= 0 && pY <= 0 && pZ <= 0)
            {
                Debug.LogError("Grid size parameters has to be positive integers");
                return false;
            }

            _gridSizeX = pX;
            _gridSizeY = pY;
            _gridSizeZ = pZ;

            _nodes = new Node[pX,pY,pZ];
            
            CreateGrid();

            return true;
        }

        private void CreateGrid()
        {
            GameObject quadContainer = new GameObject("Quads");
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = 0; x < _gridSizeX; x++)
                {
                    for (int z = 0; z < _gridSizeZ; z++)
                    {
                        var random = Random.Range(0, 11);
                        bool isWalkable = random < 9;
                        var quad = InitGridObject(x, y, z, quadContainer.transform, isWalkable);
                        Node n = new Node(x, y, z, quad, isWalkable);
                        _nodes[x, y, z] = n;
                    }
                }
            }
        }
        /// <summary>
        /// Returns a node from world position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>Node</returns>
        public Node GetNodeFromWorldPos(Vector3 pos)
        {
            if (!IsValidPos(pos))
            {
                return null;
            }

            var x = Mathf.RoundToInt(pos.x / _nodeSizeX);
            var z = Mathf.RoundToInt(pos.z / _nodeSizeZ);
            
            return IsValidPos(x,0,z) ? _nodes[x, 0, z] : null;
        }

        public Vector3 GetRandomNodePos()
        {
            int x = Random.Range(0, _gridSizeX);
            int y = Random.Range(0, _gridSizeY);
            int z = Random.Range(0, _gridSizeZ);
            return GetWorldPosFromNode(new Vector3(x,y,z));
        }

        public List<Node> GetNeighboringNodes(Node currentNode)
        {
            List<Node> neighbors = new List<Node>();
            int y = currentNode.GetNodeGridPos().y;
            for (int x = currentNode.GetNodeGridPos().x - 1; x <= currentNode.GetNodeGridPos().x + 1; x++)
            {
                for (int z = currentNode.GetNodeGridPos().z - 1; z <= currentNode.GetNodeGridPos().z + 1; z++)
                {
                    if (x == currentNode.GetNodeGridPos().x && z == currentNode.GetNodeGridPos().z)
                    {
                        continue;
                    }

                    if (IsValidPos(x, y, z))
                    {
                        neighbors.Add(_nodes[x, y, z]);
                    }
                }
            }
            return neighbors;
        }

        private Vector3 GetWorldPosFromNode(Vector3 pos)
        {
            return new Vector3(pos.x*_nodeSizeX, pos.y*_nodeSizeY, pos.z*_nodeSizeZ);
        }
        
        public bool IsOutsideGrid(Vector3 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.z < 0)
            {
                return true;
            }

            if (pos.x > _gridSizeX * _nodeSizeX - 1 || pos.y > _gridSizeY * _nodeSizeY-1 || pos.z > _gridSizeZ * _nodeSizeZ-1)
            {
                return true;
            }

            return false;
        }

        private GameObject InitGridObject(int pX, int pY, int pZ, Transform parent, bool isWalkable)
        {
            Vector3 pos = new Vector3(pX * _nodeSizeX, pY * _nodeSizeY, pZ * _nodeSizeZ);
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            if (isWalkable)
            {
                quad.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Walkable);
            }
            else
            {
                quad.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Blocked);
                var asteroid = Resources.Load<GameObject>("Prefabs/Asteroids");
                var asteroidObject = GameObject.Instantiate(asteroid, Vector3.zero, Quaternion.identity);
                asteroidObject.transform.SetParent(quad.transform);
            }
            
            quad.transform.SetParent(parent.transform);
            quad.transform.position = pos;
            quad.transform.rotation = Quaternion.Euler(90, 0, 0);
            quad.transform.localScale = new Vector3(0.98f*_nodeSizeX, 0.98f*_nodeSizeY, 0.98f*_nodeSizeZ);
            return quad;
        }
        private bool IsValidPos(int pX, int pY, int pZ)
        {
            if (pX < 0 || pY < 0 || pZ < 0)
            {
                return false;
            }
            if (pX > _nodeSizeX*_gridSizeX - 1 || pY >  _nodeSizeY*_gridSizeY-1 || pZ >  _nodeSizeZ*_gridSizeZ-1)
            {
                return false;
            }
            return true;
        }
        
        private bool IsValidPos(Vector3 pos)
        {
            return IsValidPos((int)pos.x, (int)pos.y, (int)pos.z);
        }

        public Node GetNode(int x, int y, int z)
        {
            if (x >= _gridSizeX || x < 0)
            {
                return null;
            }

            if (y >= _gridSizeY || y < 0)
            {
                return null;
            }

            if (z >= _gridSizeZ || z < 0)
            {
                return null;
            }

            return _nodes[x, y, z];
        }

        public Node GetNode(Vector3 pos)
        {
            return GetNode((int)pos.x, (int)pos.y, (int)pos.z);
        }
        
        private bool IsWalkable(int x, int y, int z)
        {
            var node = GetNode(x, y, z);
            if (node != null)
            {
                return node.isWalkable;
            }

            return false;
        }
        
        public enum DiagonalMovement
        {
            Always,
            Never,
            IfAtMostOneObstacle,
            OnlyWhenNoObstacles,
        };

        /**
        * Get the neighbors of the given node.
        *
        *     offsets      diagonalOffsets:
        *  +---+---+---+    +---+---+---+
        *  |   | 0 |   |    | 0 |   | 1 |
        *  +---+---+---+    +---+---+---+
        *  | 3 |   | 1 |    |   |   |   |
        *  +---+---+---+    +---+---+---+
        *  |   | 2 |   |    | 3 |   | 2 |
        *  +---+---+---+    +---+---+---+
        *
        *  When allowDiagonal is true, if offsets[i] is valid, then
        *  diagonalOffsets[i] and
        *  diagonalOffsets[(i + 1) % 4] is valid.
        * @param {Node} node
        * @param {DiagonalMovement} diagonalMovement
        */
        public List<Node> GetAllNeighbors(Node node, DiagonalMovement diagonalMovement)
        {
            List<Node> neighbors = new List<Node>();

            int x = node.x,
                z = node.z,
                y = node.y;

            bool s0 = false,
                d0 = false,
                s1 = false,
                d1 = false,
                s2 = false,
                d2 = false,
                s3 = false,
                d3 = false;

            // ↑
            if (IsWalkable(x, y, z + 1))
            {
                neighbors.Add(GetNode(x, y, z + 1));
                s0 = true;
            }

            // →
            if (IsWalkable(x + 1, y, z))
            {
                neighbors.Add(GetNode(x + 1, y, z));
                s1 = true;
            }

            // ↓
            if (IsWalkable(x, y, z - 1))
            {
                neighbors.Add(GetNode(x, y, z - 1));
                s2 = true;
            }

            // ←
            if (IsWalkable(x - 1, y, z))
            {
                neighbors.Add(GetNode(x - 1, y, z));
                s3 = true;
            }

            if (diagonalMovement == DiagonalMovement.Never)
            {
                return neighbors;
            }

            if (diagonalMovement == DiagonalMovement.OnlyWhenNoObstacles)
            {
                d0 = s3 && s0;
                d1 = s0 && s1;
                d2 = s1 && s2;
                d3 = s2 && s3;
            }
            else if (diagonalMovement == DiagonalMovement.IfAtMostOneObstacle)
            {
                d0 = s3 || s0;
                d1 = s0 || s1;
                d2 = s1 || s2;
                d3 = s2 || s3;
            }
            else if (diagonalMovement == DiagonalMovement.Always)
            {
                d0 = true;
                d1 = true;
                d2 = true;
                d3 = true;
            }
            else
            {
                Debug.LogError("Incorrect value of diagonalMovement");
            }

            // ↖
            if (d0 && IsWalkable(x - 1, y, z + 1))
            {
                neighbors.Add(GetNode(x - 1, y, z + 1));
            }

            // ↗
            if (d1 && IsWalkable(x + 1, y, z + 1))
            {
                neighbors.Add(GetNode(x + 1, y, z + 1));
            }

            // ↘
            if (d2 && IsWalkable(x + 1, y, z - 1))
            {
                neighbors.Add(GetNode(x + 1, y, z - 1));
            }

            // ↙
            if (d3 && IsWalkable(x - 1, y, z - 1))
            {
                neighbors.Add(GetNode(x - 1, y, z - 1));
            }

            return neighbors;
        }

        public Vector3 GetRandomWalkableNode()
        {
            int loop = 100;

            for (int i = 0; i < loop; i++)
            {
                var randomX = Random.Range(0, sizeX);
                var randomZ = Random.Range(0, sizeZ);

                var node = _nodes[randomX, 0, randomZ];
                if (node.isWalkable)
                {
                    return node.GetNodeWorldPos();
                }
            }

            Debug.LogError("Could not find walkable node");
            return Vector3.zero;
        }

        public void ResetColors()
        {
            foreach (var n in _nodes)
            {
                n.SetColor(Color.white);
            }
        }

        public IEnumerable AllNodes()
        {
            return _nodes;
        }
    }
}

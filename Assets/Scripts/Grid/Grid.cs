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
                        var random = Random.Range(0, 100);
                        bool isWalkable = random < 90;
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
        private Vector3 GetWorldPosFromNode(Vector3 pos)
        {
            return new Vector3(pos.x*_nodeSizeX, pos.y*_nodeSizeY, pos.z*_nodeSizeZ);
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
        
        public Node NodeFromWorldPosition(Vector3 worldPosition)
        {
            float worldX = worldPosition.x;
            float worldY = worldPosition.y;
            float worldZ = worldPosition.z;

            int x = Mathf.RoundToInt(worldX);
            int y = Mathf.RoundToInt(worldY);
            int z = Mathf.RoundToInt(worldZ);

            if (x > sizeX - 1)
            {
                x = sizeX - 1;
            }

            if (y > sizeY - 1)
            {
                y = sizeY - 1;
            }

            if (z > sizeZ - 1)
            {
                z = sizeZ - 1;
            }

            if (x < 0)
            {
                x = 0;
            }

            if (y < 0)
            {
                y = 0;
            }

            if (z < 0)
            {
                z = 0;
            }

            return _nodes[x, y, z];
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
                n.UpdateColor();
            }
        }

        public IEnumerable AllNodes()
        {
            return _nodes;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public struct PathfindingGrid
    {
        private PathfindingNode[,,] _nodes;
        private int _gridSizeX;
        private int _gridSizeY;
        private int _gridSizeZ;

        public PathfindingGrid(Astar.Grid grid)
        {
            _gridSizeX = grid.sizeX;
            _gridSizeY = grid.sizeY;
            _gridSizeZ = grid.sizeZ;
            _nodes = new PathfindingNode[grid.sizeX, grid.sizeY, grid.sizeZ];
            foreach (Astar.Node node in grid.AllNodes())
            {
                _nodes[node.x, node.y, node.z] = new PathfindingNode(node.x, node.y, node.z, node.isWalkable);
            }
        }

        public PathfindingNode GetNode(int x, int y, int z)
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
    }
}
using System.Collections.Generic;
using UnityEngine;
using Node = Astar.Node;

namespace Pathfinding
{
    /// <summary>
    /// Pathfinding for both A* and JumpSearch
    /// </summary>
    
    
    public class Pathfinder
    {
        private PathfindingGrid _grid;
        private readonly PathfindingNode _startPosition;
        private readonly PathfindingNode _endPosition;
        private float _targetDistance;
        public volatile bool JobDone;
        private readonly DiagonalMovement _diagonalMovement;
        private readonly bool _useJump; // Flag to use optimized jump point algorithm
        private readonly PathfindMaster.PathfindingJobComplete _completeCallback;
        private List<Vector3> _foundPath = new List<Vector3>();
        
        //We need two lists, one for the nodes we need to check and one for the nodes we've already checked
        private List<PathfindingNode> _openSet = new List<PathfindingNode>();
        private HashSet<PathfindingNode> _closedSet = new HashSet<PathfindingNode>();
        // Stack of jump search nodes
        private Stack<PathfindingNode> jumpStack = new Stack<PathfindingNode>();
        private List<PathfindingNode> _retList = new List<PathfindingNode>();
        // Store pathfinding steps for debugging
        private PathfindingSnapShot _pathfindingSnapShot;

        public Pathfinder(PathfindingGrid grid, PathfindingSnapShot pathfindingSnapShot, Node start, Node target, PathfindMaster.PathfindingJobComplete callback, DiagonalMovement diagonalMovement, bool pUseJump = false)
        {
            _grid = grid;
            _startPosition = new PathfindingNode(start.x,start.y,start.z,start.isWalkable);
            _endPosition = new PathfindingNode(target.x,target.y,target.z,target.isWalkable);
            _completeCallback = callback;
            _useJump = pUseJump;
            _pathfindingSnapShot = pathfindingSnapShot;
            _diagonalMovement = diagonalMovement;
        }
        
        // The rules for moving diagonally
        public enum DiagonalMovement {
            Always,
            Never,
            IfAtMostOneObstacle,
            OnlyWhenNoObstacles,
        };

        // Starts pathfinding
        public void FindPath()
        {
            _foundPath = _useJump ? FindJumpSearchPath(_startPosition, _endPosition) : FindPathActual(_startPosition, _endPosition);

            JobDone = true;
        }

        // Callback after finding path
        public void NotifyComplete()
        {
            _completeCallback.Invoke(_foundPath);
        }
        
        
        // Find path using A*
        private List<Vector3> FindPathActual(PathfindingNode start, PathfindingNode target)
        {
            _foundPath.Clear();
            _openSet.Clear();
            _closedSet.Clear();

            //We start adding to the open set
            _openSet.Add(start);
            
            // set the `g` and `f` value of the start node to be 0
            start.gCost = 0;
            start.hCost = 0;

            while (_openSet.Count > 0)
            {
                PathfindingNode currentNode = _openSet[0];
                
                    foreach (PathfindingNode t in _openSet)
                    {
                        //We check the costs for the current node
                        if (t.fCost < currentNode.fCost ||
                            ((int) t.fCost == (int) currentNode.fCost &&
                             t.hCost < currentNode.hCost))
                        {
                            //and then we assign a new current node
                            if (!currentNode.Equals(t))
                            {
                                currentNode = t;
                            }
                        }
                    }

                    //we remove the current node from the open set and add to the closed set
                _openSet.Remove(currentNode);
                _closedSet.Add(currentNode);

                //if the current node is the target node
                if (currentNode.x == target.x && currentNode.y == target.y && currentNode.z == target.z)
                {
                    //that means we reached our destination, so we are ready to retrace our path
                    _foundPath = RetracePath(start, currentNode);
                    break;
                }

                //if we haven't reached our target, then we need to start looking the neighbours
                foreach (PathfindingNode neighbour in GetAllNeighbors(currentNode, _diagonalMovement))
                {
                    if (!_closedSet.Contains(neighbour))
                    {
                        //we create a new movement cost for our neighbours
                        float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                        //and if it's lower than the neighbour's cost
                        if (newMovementCostToNeighbour < neighbour.gCost || !_openSet.Contains(neighbour))
                        {
                            //we calculate the new costs
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, target);
                            //Assign the parent node
                            neighbour.parentNode = currentNode;
                            //And add the neighbour node to the open set
                            if (!_openSet.Contains(neighbour))
                            {
                                _openSet.Add(neighbour);
                            }
                        }
                    }
                }
                _pathfindingSnapShot.TakeSnapshot(currentNode, _openSet, _closedSet);
            }

            //we return the path at the end
            return _foundPath;
        }
        
        // Find path using JPS (Jump point search)
        private List<Vector3> FindJumpSearchPath(PathfindingNode start, PathfindingNode target)
        {
            _foundPath.Clear();
            _openSet.Clear();
            _closedSet.Clear();

            //We start adding to the open set
            _openSet.Add(start);
            
            // set the `g` and `f` value of the start node to be 0
            start.gCost = 0;
            start.hCost = 0;

            while (_openSet.Count > 0)
            {
                PathfindingNode currentNode = _openSet[0];
                
                 foreach (PathfindingNode n in _openSet)
                 {
                     //We check the costs for the current node
                     if (n.fCost < currentNode.fCost)
                     {
                         //and then we assign a new current node
                         if (currentNode.x != n.x && currentNode.y != n.y && currentNode.z != n.z)
                         {
                             currentNode = n;
                         }
                     }
                 }

                 //we remove the current node from the open set and add to the closed set
                _openSet.Remove(currentNode);
                _closedSet.Add(currentNode);

                //if the current node is the target node
                if (currentNode.x == target.x && currentNode.y == target.y && currentNode.z == target.z)
                {
                    //that means we reached our destination, so we are ready to retrace our path
                    _foundPath = RetracePath(start, currentNode);
                    break;
                }
                
                IdentifySuccessors(currentNode);

            }

            //return the path at the end
            return _foundPath;
        }
        
        private List<Vector3> RetracePath(PathfindingNode startNode, PathfindingNode endNode)
        {
            //Retrace the path going from the endNode to the startNode
            List<Vector3> path = new List<Vector3>();
            PathfindingNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(new Vector3(currentNode.x,currentNode.y, currentNode.z));
                currentNode = currentNode.parentNode;
            }

            //reverse the list to get nodes in right order
            path.Reverse();

            return path;
        }
        /**
        * Identify successors for the given node. Runs a jump point search in the
        * direction of each available neighbor, adding any points found to the open
        * list.
        */
        private void IdentifySuccessors(PathfindingNode node)
        {
            List<PathfindingNode> jumpNeighbours = GetJumpNeighbours(node);

            foreach(var neighbor in jumpNeighbours)
            {
                if (neighbor == null)
                {
                    continue;
                }

                if (!neighbor.isWalkable)
                {
                    continue;
                }

                var jumpNode = Jump(neighbor.x, neighbor.z, node.x, node.z);
                
                if (jumpNode != null)
                {
                    if (_closedSet.Contains(jumpNode))
                    {
                        continue;
                    }

                    //we create a new movement cost for our neighbours
                    float ng = node.gCost + GetDistance(node, jumpNode);

                    //and if it's lower than the neighbour's cost
                    if (!_openSet.Contains(jumpNode) || ng < jumpNode.gCost)
                    {
                        jumpNode.gCost = ng;
                        jumpNode.hCost = GetDistance(jumpNode, _endPosition);
                        jumpNode.parentNode = node;

                        if (!_openSet.Contains(jumpNode)) {
                            _openSet.Add(jumpNode);
                        }
                    }
                }
            }
        }

        /**
         * Find the neighbors for the given node. If the node has a parent,
         * prune the neighbors based on the jump point search algorithm, otherwise
         * return all available neighbors.
         * @return List<Node> The neighbors found.
         */


        private List<PathfindingNode> GetJumpNeighbours(PathfindingNode node)
        {
            _retList.Clear();
            var parent = node.parentNode;
            int x = node.x;
            int z = node.z;
            int y = node.y;

            // directed pruning: can ignore most neighbors, unless forced.
            if (parent != null)
            {
                int px = parent.x;
                int pz = parent.z;
                // get the normalized direction of travel
                int dx = (x - px) / Mathf.Max(Mathf.Abs(x - px), 1);
                int dz = (z - pz) / Mathf.Max(Mathf.Abs(z - pz), 1);

                // search diagonally
                if (dx != 0 && dz != 0)
                {
                    // ↑ or ↓
                    if (IsWalkable(x, y, z + dz))
                    {
                        _retList.Add(GetNode(x, y, z + dz));
                    }
                    // ← or →
                    if (IsWalkable(x + dx, y, z))
                    {
                        _retList.Add(GetNode(x + dx, y, z));
                    }
                    // Path is not blocked by obstacles. Move forward diagonally
                    if (IsWalkable(x, y, z + dz) || IsWalkable(x + dx, y, z))
                    {
                        var jumpNode = GetNode(x + dx, y, z + dz);
                        if (jumpNode != null)
                        {
                            _retList.Add(jumpNode);
                        }
                    }
                    
                    if (!IsWalkable(x - dx, y, z) && IsWalkable(x, y, z + dz))
                    {
                        var jumpNode = GetNode(x - dx, y, z + dz);
                        if (jumpNode != null)
                        {
                            _retList.Add(jumpNode);
                        }
                    }
                    
                    if (!IsWalkable(x, y, z - dz) && IsWalkable(x + dx, y, z))
                    {
                        var jumpNode = GetNode(x + dx, y, z - dz);
                        if (jumpNode != null)
                        {
                            _retList.Add(jumpNode);
                        }
                    }
                }
                // search horizontally/vertically
                else
                {
                    if (dx == 0)
                    {
                        if (IsWalkable(x, y, z + dz))
                        {
                            _retList.Add(GetNode(x, y,z + dz));
                            
                            if (!IsWalkable(x + 1, y, z))
                            {
                                _retList.Add(GetNode(x + 1, y, z + dz));
                            }
                            
                            if (!IsWalkable(x - 1, y, z))
                            {
                                _retList.Add(GetNode(x - 1, y, z + dz));
                            }
                        }
                    }
                    else
                    {
                        if (IsWalkable(x + dx, y, z)) 
                        {
                            _retList.Add(GetNode(x + dx, y, z));
                            
                            if (!IsWalkable(x, y, z + 1)) {
                                _retList.Add(GetNode(x + dx, y, z + 1));
                            }
                            
                            if (!IsWalkable(x, y, z - 1)) {
                                _retList.Add(GetNode(x + dx, y, z - 1));
                            }
                        }
                    }
                }
            }
            // return all neighbors without pruning
            else
            {
                _retList = GetAllNeighbors(node, _diagonalMovement);
            }
            return _retList;
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

        /**
         * Search recursively in the direction (parent -> child), stopping only when a
         * jump point is found.
         * @return {Node} The x, z coordinate of the jump point
         *     found, or null if not found
         * https://zerowidth.com/2013/a-visual-explanation-of-jump-point-search.html
         */

        private PathfindingNode Jump(int pX, int pZ, int pPx, int pPz)
        {
            PathfindingNode next = GetNode(pX, 0, pZ);
            int dx = pX - pPx;
            int dz = pZ - pPz;
            int px = pPx;
            int pz = pPz;
            int x = pX;
            int z = pZ;

            if (next == null || !next.isWalkable)
            {
                return null;
            }

            jumpStack.Clear();
            jumpStack.Push(next);

            while (jumpStack.Count > 0)
            {
                next = jumpStack.Pop();

               _pathfindingSnapShot.TakeSnapshot(next, _openSet, _closedSet);

               if (next.x == _endPosition.x && next.y == _endPosition.y & next.z == _endPosition.z)
                {
                    return next;
                }
                
                // check for forced neighbors
                // along the diagonal
                if (dx != 0 && dz != 0)
                {
                    // if dx = 1 & dz = 1
                    // moving ↗
                    // [x][ ][ ]
                    // [ ][o][ ]
                    // [ ][ ][ ]
                    var node1 = GetNode(x - dx, 0, z + dz);

                    // [ ][ ][ ]
                    // [x][o][ ]
                    // [ ][ ][ ]
                    var node2 = GetNode(x - dx, 0, z);

                    // [ ][ ][ ]
                    // [ ][o][ ]
                    // [ ][ ][x]
                    var node3 = GetNode(x + dx, 0, z - dz);

                    // [ ][ ][ ]
                    // [ ][o][ ]
                    // [ ][x][ ]
                    var node4 = GetNode(x, 0, z - dz);

                    if ((node1 != null && node1.isWalkable && (node2 != null && !node2.isWalkable)) ||
                        (node3 != null && node3.isWalkable && (node4 != null && !node4.isWalkable)))
                    {
                        return next;
                    }

                    // when moving diagonally, must check for vertical/horizontal jump points
                    if (Jump(x + dx, z, x, z) != null || Jump(x, z + dz, x, z) != null)
                    {
                        return next;
                    }
                }
                // horizontally/vertically
                else
                {
                    if (dx != 0) // moving along x
                    {
                        // if dx = 1 & dz = 0
                        // moving →
                        // [ ][ ][x]
                        // [ ][o][ ]
                        // [ ][ ][ ]
                        var node1 = GetNode(x + dx, 0, z + 1);

                        // [ ][x][ ]
                        // [ ][o][ ]
                        // [ ][ ][ ]
                        var node2 = GetNode(x, 0, z + 1);

                        // [ ][ ][ ]
                        // [ ][o][ ]
                        // [ ][ ][x]
                        var node3 = GetNode(x + dx, 0, z - 1);

                        // [ ][ ][ ]
                        // [ ][o][ ]
                        // [ ][x][ ]
                        var node4 = GetNode(x, 0, z - 1);

                        if ((node1 != null && node1.isWalkable) && (node2 != null && !node2.isWalkable) ||
                            (node3 != null && node3.isWalkable) && (node4 != null && !node4.isWalkable))
                        {
                            return next;
                        }
                    }
                    else // moving along y
                    {
                        // if dx = 0 & dz = 1
                        // moving ↑
                        // [ ][ ][x]
                        // [ ][o][ ]
                        // [ ][ ][ ]
                        var node1 = GetNode(x + 1, 0, z + dz);
                        // [ ][ ][ ]
                        // [ ][o][x]
                        // [ ][ ][ ]
                        var node2 = GetNode(x + 1, 0, z);
                        // [x][ ][ ]
                        // [ ][o][ ]
                        // [ ][ ][ ]
                        var node3 = GetNode(x - 1, 0, z + dz);
                        // [ ][ ][ ]
                        // [x][o][ ]
                        // [ ][ ][ ]
                        var node4 = GetNode(x - 1, 0, z);

                        if (((node1 != null && node1.isWalkable) && (node2 != null && !node2.isWalkable)) ||
                            ((node3 != null && node3.isWalkable) && (node4 != null && !node4.isWalkable)))
                        {
                            return next;
                        }
                    }
                }
                
                // moving diagonally, must make sure one of the vertical/horizontal
                // neighbors is open to allow the path
                // if dx = 1 & dz = 1
                // moving ↗
                // [ ][x][ ]
                // [ ][o][x]
                // [ ][ ][ ]
                var n1 = GetNode(x + dx, 0, z);
                var n2 = GetNode(x, 0, z + dz);

                if ((n1 != null && n1.isWalkable) || (n2 != null && n2.isWalkable))
                {
                    //return Jump(x + dx, z + dz, x, z);
                    px = next.x;
                    pz = next.z;
                    x += dx;
                    z += dz;
                    if (IsWalkable(x, 0, z))
                    {
                        jumpStack.Push(GetNode(x, 0, z));
                    }
                }
            }

            return null;
        }

        private PathfindingNode JumpAlwaysDiagonal(int x, int z, int px, int pz)
        {
            PathfindingNode next = GetNode(x, 0, z);
            int dx = x - px;
            int dz = z - pz;

            if (next == null || !next.isWalkable)
            {
                return null;
            }

            if (next == _endPosition)
            {
                return next;
            }

            // check for forced neighbors
            // along the diagonal
            if (dx != 0 && dz != 0)
            {
                // if dx = 1 & dz = 1
                // moving ↗
                // [x][ ][ ]
                // [ ][o][ ]
                // [ ][ ][ ]
                var node1 = GetNode(x - dx, 0, z + dz);
                
                // [ ][ ][ ]
                // [x][o][ ]
                // [ ][ ][ ]
                var node2 = GetNode(x - dx, 0, z);
                
                // [ ][ ][ ]
                // [ ][o][ ]
                // [ ][ ][x]
                var node3 = GetNode(x + dx, 0, z - dz);
                
                // [ ][ ][ ]
                // [ ][o][ ]
                // [ ][x][ ]
                var node4 = GetNode(x, 0, z - dz);
                
                if ((node1 != null && node1.isWalkable && (node2 != null && !node2.isWalkable)) ||
                    (node3 != null && node3.isWalkable && (node4 != null && !node4.isWalkable))) {
                    return next;
                }
                // when moving diagonally, must check for vertical/horizontal jump points
                if (JumpAlwaysDiagonal(x + dx, z, x, z) != null || Jump(x, z + dz, x, z) != null) {
                    return next;
                }
            }
            // horizontally/vertically
            else
            {
                if (dx != 0) // moving along x
                {
                    // if dx = 1 & dz = 0
                    // moving →
                    // [ ][ ][x]
                    // [ ][o][ ]
                    // [ ][ ][ ]
                    var node1 = GetNode(x + 1 , 0, z + 1);
                    
                    // [ ][x][ ]
                    // [ ][o][ ]
                    // [ ][ ][ ]
                    var node2 = GetNode(x, 0, z + 1);
                    
                    // [ ][ ][ ]
                    // [ ][o][ ]
                    // [ ][ ][x]
                    var node3 = GetNode(x + dx, 0, z - 1);
                    
                    // [ ][ ][ ]
                    // [ ][o][ ]
                    // [ ][x][ ]
                    var node4 = GetNode(x, 0, z - 1);

                    if (((node1 != null && node1.isWalkable) && (node2 != null && !node2.isWalkable)) ||
                        ((node3 != null && node3.isWalkable) && (node4 != null && !node4.isWalkable)))
                    {
                        return next;
                    }
                }
                else
                {
                    // if dx = 0 & dz = 1
                    // moving ↑
                    // [ ][ ][x]
                    // [ ][o][ ]
                    // [ ][ ][ ]
                    var node1 = GetNode(x + 1, 0, z + dz);
                    // [ ][ ][ ]
                    // [ ][o][x]
                    // [ ][ ][ ]
                    var node2 = GetNode(x + 1, 0, z);
                    // [x][ ][ ]
                    // [ ][o][ ]
                    // [ ][ ][ ]
                    var node3 = GetNode(x - 1, 0, z + dz);
                    // [ ][ ][ ]
                    // [x][o][ ]
                    // [ ][ ][ ]
                    var node4 = GetNode(x - 1, 0, z);
                    
                    if (((node1 != null && node1.isWalkable) && (node2 != null &&  !node2.isWalkable)) ||
                        ((node3 != null && node3.isWalkable) && (node4 != null &&  !node4.isWalkable)))
                    {
                        return next;
                    }
                }
            }
            // Jump forward one more step
            return JumpAlwaysDiagonal(x + dx, z + dz, x, z);
        }

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
        private List<PathfindingNode> GetAllNeighbors(PathfindingNode node, DiagonalMovement diagonalMovement)
        {
            List<PathfindingNode> neighbors = new List<PathfindingNode>();
            
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
            if (IsWalkable(x, y, z + 1)) {
                neighbors.Add(GetNode(x, y, z + 1)); 
                s0 = true;
            }
            // →
            if (IsWalkable(x + 1, y, z)) {
                neighbors.Add(GetNode(x + 1, y, z));
                s1 = true;
            }
            // ↓
            if (IsWalkable(x, y, z - 1)) {
                neighbors.Add(GetNode(x, y, z - 1));
                s2 = true;
            }
            // ←
            if (IsWalkable(x - 1, y, z)) {
                neighbors.Add(GetNode(x - 1, y, z));
                s3 = true;
            }

            if (diagonalMovement == DiagonalMovement.Never) {
                return neighbors;
            }

            if (diagonalMovement == DiagonalMovement.OnlyWhenNoObstacles) {
                d0 = s3 && s0;
                d1 = s0 && s1;
                d2 = s1 && s2;
                d3 = s2 && s3;
            } else if (diagonalMovement == DiagonalMovement.IfAtMostOneObstacle) {
                d0 = s3 || s0;
                d1 = s0 || s1;
                d2 = s1 || s2;
                d3 = s2 || s3;
            } else if (diagonalMovement == DiagonalMovement.Always) {
                d0 = true;
                d1 = true;
                d2 = true;
                d3 = true;
            } else {
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
        
        private List<PathfindingNode> GetNeighbours(PathfindingNode node, bool getVerticalneighbors = false)
        {
            //This is were we start taking our neighbours
            List<PathfindingNode> neighbors = new List<PathfindingNode>();

            for (int x = -1; x <= 1; x++)
            {
                for (int yIndex = -1; yIndex <= 1; yIndex++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int y = yIndex;

                        //If we don't want a 3d A*, then we don't search the y
                        if (!getVerticalneighbors)
                        {
                            y = 0;
                        }

                        if (x == 0 && y == 0 && z == 0)
                        {
                            //000 is the current node
                        }
                        else
                        {
                            Vector3Int searchPos = new Vector3Int(node.x + x, node.y + y, node.z + z);

                            //the nodes we want are what's forward/backwards, left/right, up/down from us

                            PathfindingNode newNode = GetNeighbourNode(searchPos, false, node);
                            
                            if (newNode != null && newNode.isWalkable)
                            {
                                if (!neighbors.Contains(newNode))
                                {
                                    neighbors.Add(newNode);
                                }
                            }
                        }
                    }
                }
            }
            return neighbors;
        }

        private PathfindingNode GetNeighbourNode(Vector3Int adjPos, bool searchTopDown, PathfindingNode currentNodePos)
        {
            PathfindingNode retVal = null;
            PathfindingNode node = GetNode(adjPos.x, adjPos.y, adjPos.z);

            int originalX = adjPos.x - currentNodePos.x;
            int originalZ = adjPos.z - currentNodePos.z;

            PathfindingNode.Direction dirX = FindDirection(originalX, 0);
            PathfindingNode.Direction dirZ = FindDirection(0, originalZ);


            if (node != null && node.isWalkable)
            {
                retVal = node;
            }

            if (Mathf.Abs(originalX) == 1 && Mathf.Abs(originalZ) == 1)
            {
                PathfindingNode neighbour1 = GetNode(currentNodePos.x + originalX, currentNodePos.y, currentNodePos.z);
                PathfindingNode neighbour2 = GetNode(currentNodePos.x, currentNodePos.y, currentNodePos.z + originalZ);

                if (!ValidNode(currentNodePos, neighbour1) || !ValidNode(currentNodePos, neighbour2))
                {
                    retVal = null;
                }

                if (!ValidNode(currentNodePos, node))
                {
                    retVal = null;
                }
            }

            if (originalX != 0)
            {
                if (node != null && node.ReturnDirection(dirX).status == PathfindingNode.DirectionStatus.blocked)
                {
                    retVal = null;
                }
            }

            if (originalZ != 0)
            {
                if (node != null && node.ReturnDirection(dirZ).status == PathfindingNode.DirectionStatus.blocked)
                {
                    retVal = null;
                }
            }

            return retVal;
            }
        
        private PathfindingNode GetNode(int x, int y, int z)
        {
            return _grid.GetNode(x,y,z);
        }
        
        private int GetDistance(PathfindingNode posA, PathfindingNode posB)
        {
            //We find the distance between each node

            int distX = Mathf.Abs(posA.x - posB.x);
            int distZ = Mathf.Abs(posA.z - posB.z);
            int distY = Mathf.Abs(posA.y - posB.y);

            if (distX > distZ)
            {
                return 14 * distZ + 10 * (distX - distZ) + 10 * distY;
            }

            return 14 * distX + 10 * (distZ - distX) + 10 * distY;
        }

        private PathfindingNode.Direction FindDirection(int x, int z)
        {
            PathfindingNode.Direction retVal = PathfindingNode.Direction.e;

            if (x != 0)
            {
                retVal = (x < 0) ? PathfindingNode.Direction.e : PathfindingNode.Direction.w;
            }

            if (z != 0)
            {
                retVal = (z < 0) ? PathfindingNode.Direction.n : PathfindingNode.Direction.s;
            }

            return retVal;
        }

        private int DirectionX(PathfindingNode from, PathfindingNode to)
        {
            return to.x - from.x;
        }

        private int DirectionZ(PathfindingNode from, PathfindingNode to)
        {
            return to.z - from.z;
        }

        private bool ValidNode(PathfindingNode currentNodePos, PathfindingNode neighbour)
        {
            bool retVal = true;

            if (neighbour == null)
            {
                return false;
            }
            
            if (neighbour.isWalkable == false)
            {
                return false;
            }

            PathfindingNode.Direction x = FindDirection(DirectionX(currentNodePos, neighbour), 0);

            PathfindingNode.Direction z = FindDirection(0, DirectionZ(currentNodePos, neighbour));

            if (neighbour.ReturnDirection(x).status == PathfindingNode.DirectionStatus.blocked)
            {
                retVal = false;
            }

            if (neighbour.ReturnDirection(z).status == PathfindingNode.DirectionStatus.blocked)
            {
                retVal = false;
            }

            return retVal;
        }
        
    }
}

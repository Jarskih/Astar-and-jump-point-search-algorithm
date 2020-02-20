using System.Collections;
using System.Collections.Generic;
using Astar;
using UnityEngine;

namespace Pathfinding
{
    struct Snapshot
    {
        public PathfindingNode currentNode;
        public List<PathfindingNode> openList;
        public HashSet<PathfindingNode> closedList;

        public Snapshot(PathfindingNode currentNode, List<PathfindingNode> openList, HashSet<PathfindingNode> closedList)
        {
            this.currentNode = currentNode;
            this.openList = new List<PathfindingNode>(openList);
            this.closedList = new HashSet<PathfindingNode>(closedList);
        }
    }
    
    public class PathfindingSnapShot
    {
        
        private Astar.Grid _grid;
        private List<Snapshot> snapshots = new List<Snapshot>();
        private int index;
        public void Init(Astar.Grid grid)
        {
            _grid = grid;
        }

        public void TakeSnapshot(PathfindingNode currentNode, List<PathfindingNode> openList, HashSet<PathfindingNode> closedList)
        {
            snapshots.Add(new Snapshot(currentNode, openList, closedList));
        }

        private void VisualizeNodes()
        {
            if (index < snapshots.Count)
            {
                foreach (var node in snapshots[index].openList)
                {
                    var n = _grid.GetNode(node.x, node.y, node.z);
                    n.SetColor(Color.blue);
                    n.AddText();
                }

                foreach (var node in snapshots[index].closedList)
                {
                    var n = _grid.GetNode(node.x, node.y, node.z);
                    n.SetColor(Color.red);
                    n.AddText();
                }

                var currentNode = snapshots[index].currentNode;
                var gridnode = _grid.GetNode(currentNode.x, currentNode.y, currentNode.z);
                gridnode.SetColor(Color.green); 
            }
        }

        public void Reset()
        {
            if (snapshots.Count > 0 && index < snapshots.Count)
            {
                foreach (var node in snapshots[index].openList)
                {
                    var n = _grid.GetNode(node.x, node.y, node.z);
                    n.SetColor(Color.white);
                    n.ResetText();
                }

                foreach (var node in snapshots[index].closedList)
                {
                    var n = _grid.GetNode(node.x, node.y, node.z);
                    n.SetColor(Color.white);
                    n.ResetText();
                }
            
                var currentNode = snapshots[index].currentNode;
                var gridnode = _grid.GetNode(currentNode.x, currentNode.y, currentNode.z);
                gridnode.SetColor(Color.white);
                index++;
            }
            else
            {
                index = 0;
                snapshots.Clear();
            }
        }

        public void NextState()
        {
            Reset();
            VisualizeNodes();
        }
    }
}

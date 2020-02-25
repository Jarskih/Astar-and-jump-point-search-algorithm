using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Pathfinding;
using UnityEngine;

namespace Pathfinding
{
    public class PathfindMaster : MonoBehaviour
    {
        private Astar.Grid _grid;
        private int MaxJobs = 50;

        public delegate void PathfindingJobComplete(List<Vector3> path);

        private List<Pathfinder> _currentJobs;
        private List<Pathfinder> _todoJobs;
        private static PathfindMaster _instance;

        private PathfindingSnapShot _pathfindingSnapShot;
        public PathfindingSnapShot PathfindingSnapShot => _pathfindingSnapShot;

        public static PathfindMaster GetInstance()
        {
            return _instance;
        }

        public void Init(Astar.Grid grid)
        {
            _instance = this;
            _currentJobs = new List<Pathfinder>();
            _todoJobs = new List<Pathfinder>();
            _grid = grid;

            _pathfindingSnapShot = new PathfindingSnapShot();
            _pathfindingSnapShot.Init(grid);
        }

        /**
        * Manhattan distance.
        * @param {number} dx - Difference in x.
        * @param {number} dy - Difference in y.
        * @return {number} dx + dy
        */
        public static float Manhattan(float dx, float dz) {
            return dx + dz;
        }
        
        /**
        * Octile distance.
        * @param {number} dx - Difference in x.
        * @param {number} dy - Difference in y.
        * @return {number} sqrt(dx * dx + dy * dy) for grids
        */
        public static float Octile(float dx, float dz) {
            var f = Mathf.Sqrt( 2) - 1;
            return (dx < dz) ? f * dx + dz : f * dz + dx;
        }

        public bool JobsDone()
        {
            return _todoJobs.Count == 0;
        }

        private void Update()
        {
            if (_currentJobs == null)
            {
                _currentJobs = new List<Pathfinder>();
                _todoJobs = new List<Pathfinder>();
            }

            int i = 0;

            while (i < _currentJobs.Count)
            {
                if (_currentJobs[i].JobDone)
                {
                    _currentJobs[i].NotifyComplete();
                    _currentJobs.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (_todoJobs.Count > 0 && _currentJobs.Count < MaxJobs)
            {
                Pathfinder job = _todoJobs[0];
                _todoJobs.RemoveAt(0);
                _currentJobs.Add(job);
                
                ThreadPool.QueueUserWorkItem(
                    delegate {
                        job.FindPath(); 
                    }, null);
            }
        }

        public void RequestPathfind(Astar.Node start, Astar.Node target, PathfindingJobComplete completeCallback, Pathfinder.DiagonalMovement diagonalMovement, bool useJumpSearch = true)
        {
            Pathfinder newJob = new Pathfinder(new PathfindingGrid(_grid), _pathfindingSnapShot, start, target, completeCallback, diagonalMovement, useJumpSearch);
            _todoJobs.Add(newJob);
        }
    }
}

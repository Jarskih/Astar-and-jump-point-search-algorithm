using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Astar
{
    public class PathfindMaster : MonoBehaviour
    {
        private Grid _grid;
        private int MaxJobs = 50;

        public delegate void PathfindingJobComplete(List<Node> path);

        private List<Pathfinder> currentJobs;
        private List<Pathfinder> todoJobs;
        private static PathfindMaster _instance;

        public static PathfindMaster GetInstance()
        {
            return _instance;
        }

        public void Init(Grid grid)
        {
            _instance = this;
            currentJobs = new List<Pathfinder>();
            todoJobs = new List<Pathfinder>();
            _grid = grid;
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
            return todoJobs.Count == 0;
        }

        private void Update()
        {
            if (currentJobs == null)
            {
                currentJobs = new List<Pathfinder>();
                todoJobs = new List<Pathfinder>();
            }

            int i = 0;

            while (i < currentJobs.Count)
            {
                if (currentJobs[i].JobDone)
                {
                    currentJobs[i].NotifyComplete();
                    currentJobs.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (todoJobs.Count > 0 && currentJobs.Count < MaxJobs)
            {
                Pathfinder job = todoJobs[0];
                todoJobs.RemoveAt(0);
                currentJobs.Add(job);

                //Start a new thread

                //Thread jobThread = new Thread(job.FindPath);
                ThreadPool.QueueUserWorkItem(
                    delegate {
                        job.FindPath(); 
                    }, null);
                //jobThread.Start();

                //As per the doc
                //https://msdn.microsoft.com/en-us/library/system.threading.thread(v=vs.110).aspx
                //It is not necessary to retain a reference to a Thread object once you have started the thread. 
                //The thread continues to execute until the thread procedure is complete.               
            }
        }

        public void RequestPathfind(Node start, Node target, PathfindingJobComplete completeCallback, bool useJumpSearch = true)
        {
            Pathfinder newJob = new Pathfinder(_grid, start, target, completeCallback, useJumpSearch);
            todoJobs.Add(newJob);
        }
    }
}

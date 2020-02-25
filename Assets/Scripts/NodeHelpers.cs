using System.Collections.Generic;
using UnityEngine;

namespace Astar
{

    public static class NodeHelpers
    {
        private static Camera _cam;
        private static RaycastHit[] _results = new RaycastHit[10];

        public static Node FindNodeFromMousePosition(ref Grid grid)
        {
            Node retVal = null;

            if (_cam == null)
            {
                _cam = Camera.main;
            }

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            var size = Physics.RaycastNonAlloc(ray, _results, 1000);

            List<Node> groundNodes = new List<Node>();

            //Sorted the raycast hits, now it will return the closest ground node from the camera
            //did this because RaycastAll doesnt have a certain order of hits so sorting them makes for more accurate results

            for (int i = 0; i < _results.Length; i++)
            {
                Node n = grid.NodeFromWorldPosition(_results[i].point); // go find the node for each hit

                if (n != null)
                {
                    groundNodes.Add(n);
                }
            }

            float minDis = Mathf.Infinity;
            for (int i = 0; i < groundNodes.Count; i++)
            {
                float tmpDis = Vector3.Distance(groundNodes[i].nodeObject.transform.position,
                    _cam.transform.position);

                if (tmpDis < minDis)
                {
                    minDis = tmpDis;
                    retVal = groundNodes[i];
                }
            }

            return retVal;
        }
    }
}

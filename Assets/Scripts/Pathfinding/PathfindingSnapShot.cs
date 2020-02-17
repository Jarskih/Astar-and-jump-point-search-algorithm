using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Astar
{
    public class PathfindingSnapShot : MonoBehaviour
    {
        private static PathfindingSnapShot instance;

        private List<Node> openList = new List<Node>();
        private HashSet<Node> closedList = new HashSet<Node>();

        void Start()
        {
            instance = this;
        }

        public static PathfindingSnapShot Instance()
        {
            return instance;
        }

        public void TakeSnapshot(List<Node> openList, HashSet<Node> closedList)
        {
            this.openList = openList;
            this.closedList = closedList;
        }

        public void VisualizeNodes()
        {
            foreach (var node in this.openList)
            {
                node.SetColor(Color.blue);
                node.AddText();
            }

            foreach (var node in closedList)
            {
                node.SetColor(Color.red);
                node.AddText();
            }
        }

        public void Reset()
        {
            foreach (var node in this.openList)
            {
                node.SetColor(Color.white);
                node.ResetText();
            }

            foreach (var node in closedList)
            {
                node.SetColor(Color.white);
                node.ResetText();
            }
        }
    }
}

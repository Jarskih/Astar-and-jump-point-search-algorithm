using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Astar
{
    public class Move : MonoBehaviour
{
    public List<Node> _path = new List<Node>();
    private Grid _grid;
    private Camera _camera;
    private Node _currentNode;
    private int _currentPathIndex;

    public void Init(Grid grid)
    {
        _grid = grid;
        _camera = Camera.main;
        _currentNode = _grid.GetNodeFromWorldPos(transform.position);
    }

    // Update is called once per frame
    public void Tick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Node node = FindNodeFromMousePosition(_grid);
            if (node == null)
            {
                return;
            }
            _currentNode = _grid.GetNodeFromWorldPos(transform.position);
            PathfindMaster.GetInstance().RequestPathfind(_currentNode, node, UpdatePath, false);
        }
        
        if (_path.Count > 0)
        {
            for (int i = 0; i < _path.Count - 2; i++)
            {
                Debug.DrawLine(_path[i].GetNodeWorldPos(), _path[i + 1].GetNodeWorldPos());
            }
        }
    }
    
    public void GetPath(Entity target, bool jumpSearch)
    {
        var targetNode = _grid.GetNodeFromWorldPos(target.transform.position);
        var currentNode = _grid.GetNodeFromWorldPos(transform.position);
        PathfindMaster.GetInstance().RequestPathfind(currentNode, targetNode, UpdatePath, jumpSearch);
    }

    private void UpdatePath(List<Node> path)
    {
        _path = path;
        _currentPathIndex = 0;
    }

    public void MoveAlongPath()
    {
        var distance = Vector3.Distance(transform.position, _path[_currentPathIndex].GetNodeWorldPos());
        if(distance < 0.1f)
        {
            _currentPathIndex++;
        }

        if (_currentPathIndex == _path.Count)
        {
            StopMoving();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _path[_currentPathIndex].GetNodeWorldPos(), 0.1f);
    }

    private void StopMoving()
    {
        _path.Clear();
        _currentPathIndex = 0;
    }

    private Node FindNodeFromMousePosition(Grid grid)
    {
        Node retVal = null;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000);

        List<Node> groundNodes = new List<Node>();

        //Sorted the raycast hits, now it will return the closest ground node from the camera
        //did this because RaycastAll doesnt have a certain order of hits so sorting them makes for more accurate results

        foreach (var t in hits)
        {
            Node n = grid.GetNodeFromWorldPos(t.point); // go find the node for each hit

            if (n != null)
            {
                groundNodes.Add(n);
            }
        }

        float minDis = Mathf.Infinity;
        foreach (var t in groundNodes)
        {
            float tmpDis = Vector3.Distance(t.GetNodeWorldPos(),
                Camera.main.transform.position);

            if (tmpDis < minDis)
            {
                minDis = tmpDis;
                retVal = t;
            }
        }
        return retVal;
    }

    public bool HasPath()
    {
        return _path.Count != 0;
    }
}
}

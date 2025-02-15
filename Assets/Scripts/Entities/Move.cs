﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pathfinding;
using UnityEngine;

namespace Astar
{
    public class Move : MonoBehaviour
{
    [SerializeField]
    private Pathfinder.DiagonalMovement _diagonalMovement = Pathfinder.DiagonalMovement.IfAtMostOneObstacle;
    public List<Vector3> _path = new List<Vector3>();
    private Grid _grid;
    private Camera _camera;
    private Node _currentNode;
    private int _currentPathIndex;
    private LineRenderer _lineRenderer;
    public void Init(Grid grid, bool AIactive)
    {
        _grid = grid;
        _camera = Camera.main;
        _currentNode = _grid.GetNodeFromWorldPos(transform.position);
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;
        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.white;
        _lineRenderer.material = Resources.Load<Material>("Materials/Line");
        if (!AIactive)
        {
            _lineRenderer.enabled = false;
        }
    }

    // Update is called once per frame
    public void Tick(bool jumpSearch)
    {
        if (Input.GetMouseButtonDown(0))
        {
            PathfindMaster.GetInstance().PathfindingSnapShot.Clear();
            _grid.ResetColors();
            Node node = FindNodeFromMousePosition(_grid);
            if (node == null)
            {
                return;
            }
            _currentNode = _grid.GetNodeFromWorldPos(transform.position);
            PathfindMaster.GetInstance().RequestPathfind(_currentNode, node, UpdatePath, _diagonalMovement, jumpSearch);
        }

        // Visualize pathfinding steps if debugging
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PathfindMaster.GetInstance().PathfindingSnapShot.NextState();
        }
    }
    
    public void GetPath(Entity target, bool jumpSearch)
    {
        var targetNode = _grid.GetNodeFromWorldPos(target.transform.position);
        var currentNode = _grid.GetNodeFromWorldPos(transform.position);
        PathfindMaster.GetInstance().RequestPathfind(currentNode, targetNode, UpdatePath, _diagonalMovement, jumpSearch);
    }

    private void UpdatePath(List<Vector3> path)
    {
        _path = path;
        _currentPathIndex = 0;
        if (_path.Count > 0)
        {
            for (int i = 0; i < _path.Count; i++)
            {
                _lineRenderer.positionCount = path.Count;
                _lineRenderer.SetPosition(i, _path[i]);
            }
        }
    }

    public void MoveAlongPath()
    {
        var distance = Vector3.Distance(transform.position, _path[_currentPathIndex]);
        if(distance < 0.1f)
        {
            _currentPathIndex++;
        }

        if (_currentPathIndex == _path.Count)
        {
            StopMoving();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _path[_currentPathIndex], 0.1f);
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

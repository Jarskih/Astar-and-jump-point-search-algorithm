using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    public class Main : MonoBehaviour
    {
        private readonly int _gridSizeX = 20;
        private readonly int _gridSizeY = 1;
        private readonly int _gridSizeZ = 20;

        [SerializeField] private Grid _grid;
        private PathfindMaster _pathfindMaster;
        [SerializeField] private GameObject _starShip;
        [SerializeField] private GameObject _spaceStation;
        [SerializeField] private GameObject _fallenStar;
        void Start()
        {
            gameObject.AddComponent<EventManager>();
            _pathfindMaster = gameObject.AddComponent<PathfindMaster>();

            // Create grid
            _grid = new Grid();
            if (!_grid.Init(_gridSizeX, _gridSizeY, _gridSizeZ))
            {
                Debug.LogError("Error creating grid");
            }

            _pathfindMaster.Init(_grid);

            _starShip.GetComponent<Move>().Init(_grid);
        }

        // Update is called once per frame
        void Update() {
        }
    }
}
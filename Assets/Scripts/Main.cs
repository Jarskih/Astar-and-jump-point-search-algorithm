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

            SpawnEntity("StarChaser", Entity.EntityType.StarChaser, true);
            SpawnEntity("SpaceShip", Entity.EntityType.SpaceShip, false);
            SpawnEntity("FallenStar", Entity.EntityType.FallenStar, false);
            SpawnEntity("TradingPost", Entity.EntityType.TradingPost, false);
        }

        private void SpawnEntity(string name, Entity.EntityType type, bool aiActive)
        {
            var GO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GO.name = name;
            var entity = GO.AddComponent<Entity>();
            entity.transform.position = _grid.GetRandomWalkableNode();
            entity.Init(_grid, type, aiActive);
            EntityController.AddEntity(entity);
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var entity in EntityController.GetEnties())
            {
                entity.Tick();
            }
        }
    }
}
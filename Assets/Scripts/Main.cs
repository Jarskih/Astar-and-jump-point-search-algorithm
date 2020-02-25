using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    public class Main : MonoBehaviour
    {
        private readonly int _gridSizeX = 20;
        private readonly int _gridSizeY = 1;
        private readonly int _gridSizeZ = 20;

        private Grid _grid;
        private Pathfinding.PathfindMaster _pathfindMaster;
        private int _numberOfStars = 5;
        private EditGrid _editGrid;
        void Start()
        {
            // Add components
            gameObject.AddComponent<EventManager>();
            _pathfindMaster = gameObject.AddComponent<Pathfinding.PathfindMaster>();
            _editGrid = gameObject.AddComponent<EditGrid>();
            
            // Create grid
            _grid = new Grid();
            if (!_grid.Init(_gridSizeX, _gridSizeY, _gridSizeZ))
            {
                Debug.LogError("Error creating grid");
            }

            // Init components
            _pathfindMaster.Init(_grid);
            _editGrid.Init(_grid);
            
            // Spawn entities
            SpawnEntity("StarChaser", Entity.EntityType.StarChaser, true, "Prefabs/StarChaser");
            SpawnEntity("SpaceShip", Entity.EntityType.SpaceShip, false, "Prefabs/SpaceShip");
            for (int i = 0; i < _numberOfStars; i++)
            {
                SpawnEntity("FallenStar", Entity.EntityType.FallenStar, false, "Prefabs/FallenStar"); 
            }
            SpawnEntity("TradingPost", Entity.EntityType.TradingPost, false, "Prefabs/TradingPost");
        }

        private void SpawnEntity(string entityName, Entity.EntityType type, bool aiActive, string path)
        {
            var model = Resources.Load<GameObject>(path);
            var GO = GameObject.Instantiate(model, transform.position, Quaternion.identity);
            GO.name = entityName;
            var entity = GO.AddComponent<Entity>();
            entity.transform.position = _grid.GetRandomWalkableNode();
            entity.Init(_grid, type, aiActive);
            EntityController.AddEntity(entity);
        }

        // Update is called once per frame
        void Update()
        {
            _editGrid.Tick();
            foreach (var entity in EntityController.GetEnties())
            {
                entity.Tick();
            }
        }
    }
}
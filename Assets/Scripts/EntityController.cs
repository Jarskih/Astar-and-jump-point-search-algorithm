using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    public static class EntityController
    {
        private static List<Entity> _entities = new List<Entity>();

        public static void AddEntity(Entity entity)
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }
        
        public static void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);
        }

        /// <summary>
        /// Get first entity of right entity type. If no star entity is not found, refresh star entities.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static Entity GetEntity(Entity.EntityType entityType, Grid grid)
        {
            Entity entity = null;
            foreach (var e in _entities)
            {
                if (!e.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (e.GetEntityType() == entityType)
                {
                    entity = e;
                }   
            }

            if (entity == null)
            {
                ResetStars(grid);
                return GetEntity(entityType, grid);
            }

            return entity;
        }

        public static List<Entity> GetEnties()
        {
            return _entities;
        }

        private static void ResetStars(Grid grid)
        {
            foreach (var e in _entities)
            {
                if (!e.gameObject.activeInHierarchy)
                {
                    e.transform.position = grid.GetRandomWalkableNode();
                    e.gameObject.SetActive(true);
                }
            }
        }
    }
 }
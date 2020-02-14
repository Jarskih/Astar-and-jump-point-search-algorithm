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

        public static Entity GetEntity(Entity.EntityType entityType)
        {
            foreach (var e in _entities)
            {
                if (e.GetEntityType() == entityType)
                {
                    return e;
                }   
            }
            return null;
        }

        public static List<Entity> GetEnties()
        {
            return _entities;
        }
    }
 }
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astar
{
    public class Entity : MonoBehaviour
    {
        public enum EntityType
        {
            TradingPost,
            FallenStar,
            SpaceShip,
            StarChaser
        }
        [SerializeField] private bool carrying;
        private EntityType _entityType;
        private StateController _stateController;
        private Move _move;
        private Grid c;
        private float _fatigue;
        private float _fatigueLimit = 90;
        private Grid _grid;
        private Vector3 _targetPos;
        private Entity _target;

        public void Init(Grid grid, EntityType entityType, bool AIActive)
        {
            _entityType = entityType;
            _grid = grid;
            
            _stateController = gameObject.AddComponent<StateController>();
            _stateController.SetAIActive(AIActive);
            _move = gameObject.AddComponent<Move>();

            _stateController.Init(this);
            _move.Init(grid);
        }
        
        public void Tick()
        {
            _stateController.Tick();
            //  _move.Tick();
        }
        
        public void Decide()
        {
            _target = null;
            // Rest
            if (NeedsRest())
            {
                _target = EntityController.GetEntity(EntityType.SpaceShip);
            }
            else
            {
                _target = EntityController.GetEntity(carrying ? EntityType.TradingPost : EntityType.FallenStar);
            }

            if (_target != null)
            {
                _targetPos = _target.transform.position;
                _move.GetPath(_target, false);
            }
        }

        public void MoveAlongPath()
        {
            _fatigue += Time.deltaTime;
            _move.MoveAlongPath();
        }

        public bool HasPath()
        {
           return _move.HasPath();
        }

        public void Rest()
        {
            _fatigue -= Time.deltaTime;
            _fatigue = Mathf.Clamp(_fatigue, 0, 100);
        }
        
        /// <summary>
        /// Decisions
        /// </summary>
        /// <returns></returns>
        
        public bool CanDropItem()
        {
            return carrying && !_move.HasPath();
        }
        public bool CanPickup()
        {
            return carrying == false && !_move.HasPath();
        }

        /// <summary>
        /// Picking up items
        /// </summary>
        /// <returns></returns>

        public void Drop()
        {
            carrying = false;
        }

        public void Pickup()
        {
            carrying = true;
        }

        public bool NeedsRest()
        {
            return _fatigue > _fatigueLimit;
        }

        public bool DoneResting()
        {
            return _fatigue < 1;
        }

        public EntityType GetEntityType()
        {
            return _entityType;
        }
    }
}

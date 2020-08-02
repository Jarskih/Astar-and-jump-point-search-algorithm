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
        private Entity _carrying;
        private EntityType _entityType;
        private StateController _stateController;
        private Move _move;
        private Grid c;
        [SerializeField] float _fatigue;
        private float _fatigueLimit = 10;
        private Grid _grid;
        private Vector3 _targetPos;
        private Entity _target;
        private Vector3 _restPos;
        private bool _controlled;
        
        private Entity _tradingPost;
        private Entity _fallenStar;
        private Entity _spaceShip;
        private bool _useJumpSearch;
        private float _restSpeed = 5;
        private bool _aiActive;

        public void Init(Grid grid, EntityType entityType, bool AIActive)
        {
            _entityType = entityType;
            _grid = grid;
            
            _stateController = gameObject.AddComponent<StateController>();
            _stateController.SetAIActive(AIActive);
            _move = gameObject.AddComponent<Move>();

            _stateController.Init(this);
            _move.Init(grid, AIActive);
            _aiActive = AIActive;
        }
        
        public void Tick(bool useJumpSearch, bool debugPathfinding)
        {
            if (!_aiActive)
            {
                return;
            }
            
            _useJumpSearch = useJumpSearch;
            _controlled = debugPathfinding;

            if (_controlled)
            {
                // If debugging use mouse to move entity
                _move.Tick(_useJumpSearch);
                if (_move.HasPath())
                {
                    MoveAlongPath();
                }
            }
            else
            {
                // Use AI to move entity
                _stateController.Tick();
            }
        }

        public void Sense()
        {
            _tradingPost = EntityController.GetEntity(EntityType.TradingPost, _grid);
            _fallenStar = EntityController.GetEntity(EntityType.FallenStar, _grid);
            _spaceShip = EntityController.GetEntity(EntityType.SpaceShip, _grid);
        }
        
        public void Decide()
        {
            _target = null;
            // Rest
            if (NeedsRest())
            {
                _target = _spaceShip;
                _restPos = _target.transform.position;
            }
            else
            {
                if (carrying)
                {
                    _target = _tradingPost;
                }
                else
                {
                    _target = _fallenStar;
                }
                _targetPos = _target.transform.position;
            }

            if (_target != null)
            {
                _move.GetPath(_target, _useJumpSearch);
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
            _fatigue -= Time.deltaTime * _restSpeed;
            _fatigue = Mathf.Clamp(_fatigue, 0, 100);
        }

        public bool CanDropItem()
        {
            if (!carrying)
            {
                return false;
            }

            if (_fatigue > _fatigueLimit)
            {
                return true;
            }
            
            return Vector3.Distance(transform.position, _targetPos) < 0.1f;
        }
        public bool CanPickup()
        {
            if (carrying)
            {
                return false;
            }

            if (Vector3.Distance(transform.position, _targetPos) > 0.1f)
            {
                return false;
            }

            return !(_fatigue > _fatigueLimit);
        }

        public bool CanRest()
        {
            return Vector3.Distance(transform.position, _restPos) < 0.1f;
        }

        public void Drop()
        {
            carrying = false;
            _carrying.gameObject.SetActive(false);
            _carrying.transform.SetParent(null);
            _carrying = null;
        }

        public void Pickup()
        {
            carrying = true;
            _carrying = _target;
            _carrying.transform.SetParent(transform);
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

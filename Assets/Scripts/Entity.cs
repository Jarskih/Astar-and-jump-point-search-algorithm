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
        private float _fatigueLimit = 30;
        private Grid _grid;
        private Vector3 _targetPos;
        private Entity _target;
        private Vector3 _restPos;
        public bool controlled;
        
        [SerializeField] private Entity _tradingPost;
        [SerializeField] private Entity _fallenStar;
        [SerializeField] private Entity _spaceShip;
        [SerializeField] private bool _useJumpSearch;

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
            if (!controlled)
            {
                _stateController.Tick();
            }
            else
            {
                _move.Tick(_useJumpSearch);
                if (_move.HasPath())
                {
                    MoveAlongPath();
                }
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
            _fatigue -= Time.deltaTime;
            _fatigue = Mathf.Clamp(_fatigue, 0, 100);
        }
        
        /// <summary>
        /// Decisions
        /// </summary>
        /// <returns></returns>
        
        public bool CanDropItem()
        {
            return carrying && Vector3.Distance(transform.position, _targetPos) < 0.1f;
        }
        public bool CanPickup()
        {
            return carrying == false && Vector3.Distance(transform.position, _targetPos) < 0.1f;
        }

        public bool CanRest()
        {
            return Vector3.Distance(transform.position, _restPos) < 0.1f;
        }

        /// <summary>
        /// Picking up items
        /// </summary>
        /// <returns></returns>

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

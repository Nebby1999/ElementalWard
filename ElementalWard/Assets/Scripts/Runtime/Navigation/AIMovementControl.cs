using KinematicCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace ElementalWard
{
    [RequireComponent(typeof(CharacterMaster))]
    public class AIMovementControll : MonoBehaviour
    {
        public Transform destination;
        public float refreshTimer = 3;
        public CharacterMaster CharacterMaster { get; private set; }

        public Vector3 CurrentPos
        {
            get
            {
                if(!_obtainedBodyComponents)
                    return Vector3.zero;

                return _kinematicCharacterMotor ? _kinematicCharacterMotor.InitialSimulationPosition : _body.transform.position;
            }
        }
        private bool _obtainedBodyComponents = false;
        private CharacterBody _body;
        private CharacterInputBank _bodyInputBank;
        private ICharacterMovementController _iCharacterMovementController;
        private KinematicCharacterMotor _kinematicCharacterMotor;
        private NavMeshPath _currentPath;
        private float _stopwatch;
        private bool hasGroundedController;
        private bool hasFlyingController;

        private int _pathIndex;
        private Vector3 _currentDestination;
        private Vector3 _movementDirection;
        private float _distanceFromDestination;
        [SerializeField]private Vector3[] _pathCorners = Array.Empty<Vector3>();
        private Vector3 _lastValidNavmeshPosition;
        private Vector3 _lastValidDestinationPosition;

        public void Awake()
        {
            CharacterMaster = GetComponent<CharacterMaster>();
            CharacterMaster.OnBodySpawned += GetBodyComponents; ;
        }

        private void GetBodyComponents(CharacterBody obj)
        {
            _body = obj;
            _bodyInputBank = obj.GetComponent<CharacterInputBank>();
            _iCharacterMovementController = obj.GetComponent<ICharacterMovementController>();
            if(_iCharacterMovementController != null)
            {
                _kinematicCharacterMotor = _iCharacterMovementController.Motor;
                hasFlyingController = _iCharacterMovementController is FlyingCharacterMovementController;
                hasGroundedController = _iCharacterMovementController is GroundedCharacterMovementController;
            }
            _obtainedBodyComponents = true;
        }

        private void FixedUpdate()
        {
            if (!_obtainedBodyComponents)
                return;

            if(NavMesh.SamplePosition(CurrentPos, out var hit, 25, ~0))
            {
                _lastValidNavmeshPosition = hit.position;
            }
            if(NavMesh.SamplePosition(destination.position, out hit, 25, ~0))
            {
                _lastValidDestinationPosition = hit.position;
            }

            _stopwatch += Time.fixedDeltaTime;
            if(_stopwatch > refreshTimer)
            {
                _stopwatch = 0;
                GenerateNewPath();
            }

            if (_pathCorners.Length == 0)
                return;


            ProcessPath();
        }

        private void GenerateNewPath()
        {
            _currentPath ??= new NavMeshPath();
            if(hasFlyingController)
            {
                GenerateNewPathForFlyingController();
                return;
            }
            if(hasGroundedController)
            { 
            }
            NavMesh.CalculatePath(CurrentPos, destination.position, ~0, _currentPath);
            if (_currentPath.status == NavMeshPathStatus.PathInvalid)
            {
                var modifiedDestination = new Vector3(destination.position.x, CurrentPos.y, destination.position.z);
                NavMesh.CalculatePath(CurrentPos, modifiedDestination, ~0, _currentPath);
            }
            _pathCorners = _currentPath.corners;
            for(int i = 0; i < _pathCorners.Length; i++)
            {
                var orig = _pathCorners[i];
                _pathCorners[i] = new Vector3(orig.x, orig.y + (_kinematicCharacterMotor.Capsule.bounds.size.y / 2), orig.z);
            }
            _pathIndex = 0;
        }

        private void GenerateNewPathForFlyingController()
        {
            NavMesh.CalculatePath(_lastValidNavmeshPosition, _lastValidDestinationPosition, ~0, _currentPath);
            _pathCorners = _currentPath.corners;
            for(int i = 0; i < _pathCorners.Length; i++)
            {
                var orig = _pathCorners[i];
                var nextIndex = i + 1;
                if (nextIndex > _pathCorners.Length - 1)
                {
                    nextIndex = _pathCorners.Length - 1;
                }
                ModifyVector(ref orig, _pathCorners[nextIndex]);
                orig.y += _kinematicCharacterMotor.Capsule.height;
                _pathCorners[i] = orig;
            }
            _pathIndex = 0;
        }

        private void ModifyVector(ref Vector3 start, Vector3 dest)
        {
            var vector = dest - start;
            var normalized = vector.normalized;
            float distance = Vector3.Distance(start, dest);
            var capsule = _kinematicCharacterMotor.Capsule;
            Vector3 point1 = start + capsule.center + Vector3.up * -capsule.height * 0.5f;
            Vector3 point2 = point1 + Vector3.up * capsule.height;
            Ray ray = new Ray(start, normalized);
            if(Physics.CapsuleCast(point1, point2, capsule.radius, normalized, out var hit, distance, LayerIndex.world.Mask))
            {
                Debug.Log("Oops");
                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.position = hit.point;
            }
        }

        private void ProcessPath()
        {
            _currentDestination = _pathCorners[_pathIndex];
            _distanceFromDestination = Vector3.Distance(CurrentPos, _currentDestination);
            if(_distanceFromDestination < 0.7)
            {
                _pathIndex++;
                if (_pathIndex >= _pathCorners.Length - 1)
                    _pathIndex = _pathCorners.Length - 1;
                return;
            }

            var vector = _currentDestination - CurrentPos;
            _movementDirection = vector.normalized;

            if (_bodyInputBank)
            {
                _bodyInputBank.moveVector = _movementDirection;
                var lookRot = Quaternion.LookRotation(_movementDirection, _kinematicCharacterMotor.CharacterUp);
                var lookRotEuler = lookRot.eulerAngles;
                var modified = Quaternion.Euler(0, lookRotEuler.y, 0);
                _bodyInputBank.LookRotation = modified;
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if(_pathCorners.Length > 0)
            {
                for(int i = 0; i < _currentPath.corners.Length; i++)
                {
                    Vector3 pos = _currentPath.corners[i];
                    Gizmos.DrawWireSphere(pos, 1);
                }
                Gizmos.color = Color.green;
                Gizmos.DrawCube(_currentDestination, Vector3.one);
                //Debug.DrawLine(CurrentPos, CurrentPos + (_movementDirection * 4), Color.red, 0.1f);
            }
        }
    }
}

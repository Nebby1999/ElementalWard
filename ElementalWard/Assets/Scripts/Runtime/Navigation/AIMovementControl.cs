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
        private CapsuleCollider _motorCapsule;
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
                _motorCapsule = _iCharacterMovementController.Motor.Capsule;
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
                GenerateNewPathForGroundedController();
                return;
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
            _pathIndex = 0;
            var maxIndex = _pathCorners.Length - 1;
            //Offset path corners so it's on the air.
            for(int i = 0; i < _pathCorners.Length; i++)
            {
                var orig = _pathCorners[i];
                //Offset by capsule height
                var capsuleHeight = _motorCapsule.height;
                orig.y += capsuleHeight + CurrentPos.y - capsuleHeight;
                //If we're getting close to the target, start moving vertically towards it.
                if(i > maxIndex - 3)
                {
                    var destinationY = destination.position.y;
                    float iAsFloat = i;
                    float maxIndexAsFloat = maxIndex;
                    float num = iAsFloat / maxIndexAsFloat;
                    float dividend = 3 / num;
                    destinationY /= dividend;
                    orig.y += destinationY;
                }
                _pathCorners[i] = orig;
            }

            //Ensure the capsule's dimensions can go thru the desired path
            for(int i = 0; i < _pathCorners.Length; i++)
            {
                var start = _pathCorners[i];
                var nextIndex = i + 1;
                if (nextIndex > maxIndex)
                {
                    nextIndex = maxIndex;
                }
                var dest = _pathCorners[nextIndex];
                ModifyVector(ref start, i, ref dest, nextIndex);
                _pathCorners[i] = start;
                _pathCorners[nextIndex] = dest;
            }
        }

        private void GenerateNewPathForGroundedController()
        {
            NavMesh.CalculatePath(_lastValidNavmeshPosition, _lastValidDestinationPosition, ~0, _currentPath);
            _pathCorners = _currentPath.corners;
            //Offset by half of capsule height
            for(int i = 0; i < _pathCorners.Length; i++)
            {
                var orig = _pathCorners[i];
                orig.y += _motorCapsule.height / 2;
                _pathCorners[i] = orig;
            }

            for(int i = 0; i < _pathCorners.Length; i++)
            {
                var start = _pathCorners[i];
                var nextIndex = i + 1;
                if(nextIndex > _pathCorners.Length - 1)
                {
                    nextIndex = _pathCorners.Length - 1;
                }
                var dest = _pathCorners[nextIndex];
                ModifyVector(ref start, i, ref dest, nextIndex);
                _pathCorners[i] = start;
                _pathCorners[nextIndex] = dest;
            }
            _pathIndex = 0;
        }

        private void ModifyVector(ref Vector3 start, int startIndex, ref Vector3 dest, int nextIndex)
        {
            var vector = dest - start;
            var normalized = vector.normalized;
            float distance = Vector3.Distance(start, dest);
            var capsule = _kinematicCharacterMotor.Capsule;
            var capsuleRadius = capsule.radius * 1.1f;

            Vector3 centerOfSphere1 = start + Vector3.up * (capsuleRadius + Physics.defaultContactOffset);
            Vector3 centerOfSphere2 = start + Vector3.up * (capsule.height - capsuleRadius + Physics.defaultContactOffset);
            if(Physics.CapsuleCast(centerOfSphere1, centerOfSphere2, capsuleRadius, normalized, out var hit, distance, LayerIndex.world.Mask))
            {
                var slack = hit.normal * capsuleRadius;
                if(startIndex > 0)
                    start += slack;
                
                dest += slack;
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
                _bodyInputBank.LookRotation = lookRot;
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
                    Gizmos.DrawWireSphere(pos, 0.5f);
                }
                Gizmos.color = Color.green;
                Gizmos.DrawCube(_currentDestination, Vector3.one * 0.5f);
                //Debug.DrawLine(CurrentPos, CurrentPos + (_movementDirection * 4), Color.red, 0.1f);
            }
        }
    }
}

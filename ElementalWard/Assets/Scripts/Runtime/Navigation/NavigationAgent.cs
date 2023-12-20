using Nebula;
using Nebula.Navigation;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using ReadOnly = Nebula.ReadOnlyAttribute;

namespace ElementalWard.Navigation
{
    public class NavigationAgent : MonoBehaviour
    {
        public const float TIME_BETWEEN_NAVIGATION_UPDATE = 0.5f;
        public static List<NavigationAgent> _activeAgents { get; private set; }
        public Vector3 TargetPos => _targetOverride ?? NavigationDataProvider.Target;
        private Vector3? _targetOverride;
        public INavigationAgentDataProvider NavigationDataProvider { get; private set; }
        public Vector3 CurrentPathfindingMovementVector { get; private set; }
        public Quaternion CurrentPathfindingLookRotation { get; private set; }
        public bool AskForPath
        {
            get
            {
                return _navigationCoroutine == null && _askForPath;
            }
            set
            {
                _askForPath = value;
                if(_askForPath)
                {
                    InstanceTracker.Add(this);
                }
                else
                {
                    InstanceTracker.Remove(this);
                }
            }
        }
#if UNITY_EDITOR
        [SerializeField, ReadOnly]
#endif
        private bool _askForPath = false;

#if UNITY_EDITOR
        public bool _drawPath;
        [SerializeField, ReadOnly]
#endif
        private List<Vector3> _path = new List<Vector3>();
        private int _pathIndex;
        private Vector3 _currentWaypoint;
        private float _distanceFromCurrentWaypoint;
        private bool _isStopped;
        private static GlobalNavigationAgentUpdater _updater;
        private Coroutine _navigationCoroutine;

        public void UpdatePath(NativeList<float3> newPath)
        {
            _path.Clear();
            for (int i = 0; i < newPath.Length; i++)
            {
                _path.Add(newPath[i]);
            }
            _pathIndex = 1;
            if (_pathIndex > _path.Count - 1)
                _pathIndex = _path.Count - 1;
        }

        [SystemInitializer]
        private static void Init()
        {
            _updater = new GlobalNavigationAgentUpdater();
        }
        private void Awake()
        {
            NavigationDataProvider = GetComponent<INavigationAgentDataProvider>();
        }

        private void OnEnable()
        {
            AskForPath = true;
        }
        private void OnDisable()
        {
            AskForPath = false;
        }

        public void StartNavigationCoroutine(SceneNavigationSystem.PathRequest pathRequest)
        {
            _navigationCoroutine = StartCoroutine(C_GetNavigationResults(pathRequest));
        }

        public void UpdateFromAI(float deltaTime)
        {
            if(AskForPath)
            {
                UpdateNodePath();
            }
            else
            {
                UpdateNormalPath();
            }
            /*
            if (_path.Count == 0)
            {
                CurrentPathfindingMovementVector = Vector3.zero;
                CurrentPathfindingLookRotation = NavigationDataProvider?.AgentTransform ? NavigationDataProvider.AgentTransform.rotation : Quaternion.identity;
            }

            if (!NavigationDataProvider.AgentTransform)
                return;

            if (_pathIndex < 0)
                return;

            if (!_isStopped)
            { 
                ProcessPath();
            }*/
        }

        private void UpdateNodePath()
        {
            if (_path.Count == 0)
            {
                CurrentPathfindingMovementVector = Vector3.zero;
                CurrentPathfindingLookRotation = NavigationDataProvider?.AgentTransform ? NavigationDataProvider.AgentTransform.rotation : Quaternion.identity;
            }

            if (!NavigationDataProvider.AgentTransform)
                return;

            if (_pathIndex < 0)
                return;

            if (!_isStopped)
            {
                ProcessPath();
            }
        }

        private void UpdateNormalPath()
        {
            if (!NavigationDataProvider.AgentTransform)
                return;

            if (_isStopped)
            {
                CurrentPathfindingMovementVector = Vector3.zero;
                CurrentPathfindingLookRotation = NavigationDataProvider?.AgentTransform ? NavigationDataProvider.AgentTransform.rotation : Quaternion.identity;
                return;
            }

            var vector = NavigationDataProvider.Target - NavigationDataProvider.AgentTransform.position;
            var movementDirection = vector.normalized;
            CurrentPathfindingMovementVector = movementDirection;
            CurrentPathfindingLookRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        }

        public void CancelCurrentPath()
        {
            _path.Clear();
            _pathIndex = -1;
        }

        public void Stop() => _isStopped = true;
        public void Resume() => _isStopped = false;

        private void ProcessPath()
        {

            _currentWaypoint = _path[_pathIndex];
            _distanceFromCurrentWaypoint = math.distancesq(_currentWaypoint, NavigationDataProvider.AgentTransform.position);
            if(_distanceFromCurrentWaypoint < 0.35f)
            {
                _pathIndex++;
                var num = _path.Count - 1;
                if (_pathIndex >= num)
                    _pathIndex = num;
                return;
            }

            var vector = _currentWaypoint - NavigationDataProvider.AgentTransform.position;
            var movementDirection = vector.normalized;
            CurrentPathfindingMovementVector = movementDirection;
            CurrentPathfindingLookRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        }

        private IEnumerator C_GetNavigationResults(SceneNavigationSystem.PathRequest pathRequest)
        {
            var requestResult = new SceneNavigationSystem.PathRequestResult();
            yield return SceneNavigationSystem.RequestPathAsync(pathRequest, requestResult);
            var findPathJob = requestResult.findPathJob;
            var handle = requestResult.ScheduleFindPathJob();
            
            while (!handle.IsCompleted)
            {
                yield return null;
            }
            handle.Complete();
            try
            {
                UpdatePath(findPathJob.result);
            }
            catch(System.Exception e)
            { 
                Debug.LogError(e);
            }
            finally
            {
                findPathJob.result.Dispose();
                requestResult.Dispose();
            }
            _navigationCoroutine = null;
            yield break;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawPath)
                return;

            var previousPos = Vector3.zero;
            int pathCount = _path.Count;
            for (int i = 0; i < pathCount; i++)
            {
                var pos = _path[i];
                if (i == 0)
                {
                    Gizmos.color = Color.red;
                }
                else if (i == pathCount - 1)
                {
                    Gizmos.color = Color.green;
                }
                else if (i == _pathIndex)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawSphere(_path[i], 0.5f);

                if (i == 0)
                {
                    previousPos = pos;
                    continue;
                }

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(previousPos, pos);
                previousPos = pos;
            }
        }
#endif
    }

    public interface INavigationAgentDataProvider
    {
        public float AgentHeight { get; }
        public float AgentRadius { get; }
        public Vector3 Target { get; }
        public Vector3 StartPosition { get; }
        public Transform AgentTransform { get; }
        public bool IsFlying { get; }
    }
}
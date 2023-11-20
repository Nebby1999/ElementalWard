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
        public const float TIME_BETWEEN_NAVIGATION_UPDATE = 0.1f;
        public static List<NavigationAgent> _activeAgents { get; private set; }
        public INavigationAgentDataProvider NavigationDataProvider { get; private set; }
        public Vector3 CurrentPathfindingMovementVector { get; private set; }
        public Quaternion CurrentPathfindingLookRotation { get; private set; }
        public bool AskForPath
        {
            get => _askForPath;
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
        private static GlobalNavigationAgentUpdater _updater;

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

        public IEnumerator C_GetNavigationResults(SceneNavigationSystem.PathRequest pathRequest)
        {
            InstanceTracker.Remove(this);
            var requestResult = new SceneNavigationSystem.PathRequestResult();
            Debug.Log("Requesting Path");
            yield return SceneNavigationSystem.RequestPathAsync(pathRequest, requestResult);
            Debug.Log("Path Requested");
            var findPathJob = requestResult.findPathJob;
            Debug.Log("Scheduling");
            var handle = requestResult.ScheduleFindPathJob();
            
            while (!handle.IsCompleted)
            {
                Debug.Log("Find path job not completed, waiting.");
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
            InstanceTracker.Add(this);
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
        public Vector3 Start { get; }
        public bool IsAgentFlying { get; }
    }
}
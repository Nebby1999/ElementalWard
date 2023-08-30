using Nebula.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    public class PathfindingDummy : MonoBehaviour
    {
        /*public Transform target;
        public float updateTimer;

        private float _stopwatch;

        [SerializeField, Nebula.ReadOnly]
        private List<Vector3> path;
        public void FixedUpdate()
        {
            _stopwatch += Time.fixedDeltaTime;
            if(_stopwatch >= updateTimer)
            {
                _stopwatch = 0;
                AskForPath();
            }
        }

        private void AskForPath()
        {
            var request = new NavigationSystem.PathRequest
            {
                start = transform.position,
                end = target.position,
                graphProvider = NavigationSystem.Instance.groundNodes,
            };

            this.path.Clear();
            var path = NavigationSystem.Instance.RequestPath(request);
            for(int i = 0; i < path.result.Length; i++)
            {
                this.path.Add(path.result[i]);
            }
            path.result.Dispose();
        }

        private void OnDrawGizmos()
        {
            var previousPos = Vector3.zero;
            int pathCount = path.Count;
            for(int i = 0; i < pathCount; i++)
            {
                var pos = path[i];
                if(i == 0)
                {
                    Gizmos.color = Color.green;
                }
                else if(i == path.Count -1)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                Gizmos.DrawSphere(path[i], 0.25f);

                if(i == 0)
                {
                    previousPos = pos;
                    continue;
                }

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(previousPos, pos);
                previousPos = pos;

            }
        }*/
    }
}

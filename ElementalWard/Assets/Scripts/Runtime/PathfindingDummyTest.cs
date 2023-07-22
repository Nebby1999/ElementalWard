using ElementalWard.Navigation;
using Nebula;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ElementalWard
{
    [ExecuteAlways]
    public class PathfindingDummyTest : MonoBehaviour
    {
        public AStarNodeGrid ground;
        public Transform start;
        public Transform end;

        public List<Vector3> positions = new List<Vector3>();
        [Nebula.ReadOnly]
        public bool draw;
#if UNITY_EDITOR
        [ContextMenu("EnableOrDisable Dummy")]
        public void EnableDisable()
        {
            if(!draw)
            {
                draw = true;
                EditorApplication.update -= FixedUpdate;
                EditorApplication.update += FixedUpdate;
            }
            else
            {
                draw = false;
                EditorApplication.update -= FixedUpdate;
            }
        }
        public void FixedUpdate()
        {
            var job = PathfindingStatic.RequestPath(ground, start.position, end.position, 1, 1, 1);
            job.Schedule().Complete();

            UpdateList(job.result);
            job.result.Dispose();
        }

        public void UpdateList(NativeList<float3> result)
        {
            positions.Clear();
            for (int i = 0; i < result.Length; i++)
            {
                positions.Add(result[i]);
            }
        }

        public void OnDrawGizmos()
        {
            var previousPos = Vector3.zero;
            int pathCount = positions.Count;
            for (int i = 0; i < pathCount; i++)
            {
                var pos = positions[i];
                if (i == 0)
                {
                    Gizmos.color = Color.red;
                }
                else if (i == pathCount - 1)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawSphere(positions[i], AStarNodeGrid.NODE_RADIUS);

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
}

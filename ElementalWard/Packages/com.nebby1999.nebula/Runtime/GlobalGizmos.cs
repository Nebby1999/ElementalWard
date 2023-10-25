#if DEBUG
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Nebula
{
    [ExecuteAlways]
    public class GlobalGizmos : SingletonBehaviour<GlobalGizmos>
    {
        public override bool DestroyIfDuplicate => true;
        private static List<DrawRequest> drawRequests = new List<DrawRequest>();
        private static List<DrawRequest> expiredCalls = new List<DrawRequest>();

        public static void EnqueueGizmoDrawing(Action action, int drawCalls = 60)
        {
            if(!Instance)
            {
                if (Application.isPlaying)
                {
                    var go = new GameObject();
                    go.name = "DEBUG_GlobalGizmos";
                    go.AddComponent<GlobalGizmos>();
                    DontDestroyOnLoad(go);
                }
                else
                {
                    var go = new GameObject();
                    go.name = "DEBUG_GlobalGizmmos";
                    go.AddComponent<GlobalGizmos>();
                    go.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                }
            }

            drawRequests.Add(new DrawRequest
            {
                gizmoDrawingMethod = action,
                remainingDrawCalls = drawCalls,
            });
        }

        public void OnDrawGizmos()
        {
            expiredCalls.Clear();
            for(int i = 0; i < drawRequests.Count; i++)
            {
                var request = drawRequests[i];
                request.gizmoDrawingMethod();
                request.remainingDrawCalls--;
                if(request.remainingDrawCalls <= 0)
                {
                    expiredCalls.Add(request);
                }
            }

            foreach(var request in expiredCalls)
            {
                drawRequests.Remove(request);
            }
        }

        private class DrawRequest
        {
            public Action gizmoDrawingMethod;
            public int remainingDrawCalls;
        }
    }
}
#endif
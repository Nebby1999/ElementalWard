using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Nebula.Navigation;
using System.Linq;
using Unity.Mathematics;

namespace Nebula.Editor
{
    //[CustomEditor(typeof(NodeGraph))]
    /*public class NodeGraphInspector : IMGUIInspector<NodeGraph>
    {
        public List<SerializedPathNode> nodes;
        protected bool HasGraphAsset => TargetType.NodeGraphAsset;
        protected virtual int RaycastLayers => Physics.AllLayers;
        private Ray ray;
        protected RaycastHit hitInfo;
        private int controlID;
        protected override void DrawGUI()
        {
            if(!TargetType.NodeGraphAsset)
            {
                EditorGUILayout.HelpBox("Select a valid Node Graph Asset", MessageType.Info);
            }

            IMGUIUtil.DrawCheckableProperty(serializedObject.FindProperty("graphAsset"), OnGraphAssetSet, true);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            IMGUIUtil.ConditionalButtonAction(Bake, HasGraphAsset, "Bake Nodes");
            IMGUIUtil.ConditionalButtonAction(Clear, HasGraphAsset, "Clear Nodes");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        protected virtual void OnSceneGUI()
        {
            if (!HasGraphAsset)
                return;

            nodes = TargetType.GetSerializedPathNodes();
            controlID = GUIUtility.GetControlID(FocusType.Keyboard | FocusType.Passive);

            Event evt = Event.current;
            if(evt.GetTypeForControl(controlID) == EventType.KeyDown)
            {
                OnKeyDown(evt);
            }

            Vector2 guiPos = evt.mousePosition;
            ray = HandleUtility.GUIPointToWorldRay(guiPos);

            if(Physics.Raycast(ray, out hitInfo, float.MaxValue, RaycastLayers, QueryTriggerInteraction.Collide))
            {
                hitInfo.point += TargetType.NodeGraphAsset ? TargetType.NodeGraphAsset.NodeOffset : Vector3.zero;
            }
            DrawNodes(evt);
            DrawSceneGUI(evt);
        }

        protected virtual void OnKeyDown(Event evt)
        {
            switch(evt.keyCode)
            {
                case KeyCode.B:
                    TargetType.AddNewNode(hitInfo.point);
                    evt.Use();
                    break;
                case KeyCode.N:
                    TargetType.AddNewNode(Camera.current.transform.position);
                    evt.Use();
                    break;
                case KeyCode.M:
                    TargetType.RemoveNearestNode(hitInfo.point);
                    evt.Use();
                    break;
            }
        }

        protected virtual void DrawNodes(Event evt)
        {
            if(nodes.Count > 0)
            {
                bool inRange = false;
                foreach (var serializedNode in nodes)
                {
                    if (math.any(math.isnan(serializedNode.worldPosition)))
                        continue;

                    if(hitInfo.collider != null)
                    {
                        var dist = Vector3.Distance(serializedNode.worldPosition, hitInfo.point);
                        var dir = hitInfo.point - serializedNode.worldPosition;
                        dir = dir.normalized;
                        if(Physics.Raycast(serializedNode.worldPosition, dir, out var losHit, dist, RaycastLayers))
                        {
                            if (Vector3.Angle(hitInfo.normal, dir) >= 90)
                                continue;
                        }


                        if (Vector3.Distance(serializedNode.worldPosition, hitInfo.point) <= SerializedPathNode.MAX_DISTANCE)
                        {
                            Handles.color = Color.yellow;
                            Handles.DrawLine(serializedNode.worldPosition, hitInfo.point, 2);
                            inRange = true;
                        }
                    }
                }
                Handles.color = inRange ? Color.yellow : Color.red;
            }
            else
            {
                Handles.color = Color.yellow;
            }
             
            Handles.CylinderHandleCap(controlID, hitInfo.point, Quaternion.Euler(90, 0, 0), 1, EventType.Repaint);

            for(int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (math.any(math.isnan(node.worldPosition)))
                    continue;

                Vector3 diff = node.worldPosition - ray.origin;
                if (diff.sqrMagnitude > 22500)
                    continue;

                if(node.serializedPathNodeLinkIndices.Count <= 0)
                {
                    Handles.color = Color.yellow;
                }
                else
                {
                    Handles.color = Color.green;
                }
                float scale = 1f;
                Handles.CylinderHandleCap(controlID, node.worldPosition, Quaternion.Euler(90, 0, 0), scale, EventType.Repaint);

                var pos = node.worldPosition;
                pos.y += 1;
                Handles.color = Color.white;
                Handles.Label(pos, i.ToString(), new GUIStyle(GUI.skin.GetStyle("SettingsHeader")));

                var nodeXZ = new Vector3(node.worldPosition.x, 0, node.worldPosition.z);
                var rayOriginXZ = new Vector3(ray.origin.x, 0, ray.origin.z);
                if(Vector3.Distance(nodeXZ, rayOriginXZ) < 10)
                {
                    node.worldPosition = Handles.PositionHandle(node.worldPosition, Quaternion.identity);
                }

                Handles.color = Color.magenta;

                var links = TargetType.GetSerializedPathLinks();
                foreach(var linkIndex in node.serializedPathNodeLinkIndices)
                {
                    var link = links[linkIndex];
                    if (link.nodeBIndex != -1)
                    {
                        var endNode = nodes[link.nodeBIndex];
                        if (math.any(math.isnan(endNode.worldPosition)))
                            continue;

                        Handles.DrawLine(node.worldPosition, nodes[link.nodeBIndex].worldPosition, 2);
                    }
                }
            }
        }

        protected virtual void DrawSceneGUI(Event evt)
        {
            Handles.BeginGUI();
            EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
            EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
            EditorGUILayout.BeginVertical("box", GUILayout.Width(400));

            EditorGUILayout.LabelField($"Camera Position: {Camera.current.transform.position}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Press B to add a node at Current mouse positon");
            EditorGUILayout.LabelField("Press N to add a node at the Camera Position");
            EditorGUILayout.LabelField("Press M to remove the nearest map node at cursor positon");

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            Handles.EndGUI();
        }
        private void OnGraphAssetSet(SerializedProperty prop)
        {

        }

        private void Bake()
        {
            TargetType.Bake();
        }

        private void Clear()
        {
            if (EditorUtility.DisplayDialog("WARNING: Clear All Nodes", "Clicking this button will delete EVERY node. Are you sure you want to do this?", "Yes, Im sure", "No, Take me back"))
            {
                TargetType.Clear();
            }
        }
    }*/
}

using Nebula.Navigation;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Nebula.Editor.Inspectors
{
    /// <summary>
    /// A Drawer class intended for monobehaviours inheriting from <see cref="IGraphProvider"/>, includes a Node Placer utility.
    /// </summary>
    public class GraphProviderDrawer
    {
        protected int WorldLayerIndex { get; init; }
        protected Vector3 GraphProviderPosition
        {
            get
            {
                return _transform ? _transform.position : Vector3.zero;
            }
        }
        protected Vector3 GraphProviderScale
        {
            get
            {
                return _transform ? _transform.lossyScale : Vector3.zero;
            }
        }

        protected Quaternion GraphProviderRotation
        {
            get
            {
                return _transform ? _transform.rotation : Quaternion.identity;
            }
        }
        protected IGraphProvider GraphProvider { get; init; }
        protected bool HasGraphAsset => GraphProvider.NodeGraph != null;
        protected List<SerializedPathNode> nodes;
        protected int ControlID { get; private set; }
        protected RaycastHit HitInfo { get; private set; }
        private Ray ray;
        private Transform _transform;

        public void DrawIMGUIButtons()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            IMGUIUtil.ConditionalButtonAction(Bake, HasGraphAsset, "Bake Nodes");
            IMGUIUtil.ConditionalButtonAction(Clear, HasGraphAsset, "Clear Nodes");
            EditorGUILayout.EndHorizontal();
            IMGUIUtil.ConditionalButtonAction(AdjustToGround, HasGraphAsset, "Adjust to Ground");
            EditorGUILayout.EndVertical();
        }

        public void SceneGUI()
        {
            if (!HasGraphAsset)
                return;

            Event evt = Event.current;
            nodes = GraphProvider.GetSerializedPathNodes();
            if(!EditorApplication.isPlaying)
            { 
                ControlID = GUIUtility.GetControlID(FocusType.Passive | FocusType.Keyboard);

                if(evt.GetTypeForControl(ControlID) == EventType.KeyDown)
                {
                    OnKeyDown(evt);
                }

                Vector2 guiPos = evt.mousePosition;
                ray = HandleUtility.GUIPointToWorldRay(guiPos);

                Physics.SyncTransforms();
                var hitSomething = Physics.Raycast(ray, out var hInfo, float.MaxValue, WorldLayerIndex);
                if (hitSomething)
                {
                    hInfo.point = EditorMath.RoundToNearestGrid(hInfo.point);
                    hInfo.point += GraphProvider?.NodeGraph?.NodeOffset ?? Vector3.zero;
                    HitInfo = hInfo;
                    OnSceneGUIRaycastHit();
                }
                else
                {
                    HitInfo = default;
                }
            }

            DrawNodes(evt);
            DrawSceneGUI(evt);
        }

        protected virtual void OnSceneGUIRaycastHit()
        {

        }
        protected virtual void Bake()
        {
            if (GraphProvider.NodeGraph is UnityEngine.Object obj)
                Undo.RegisterCompleteObjectUndo(obj, "Bake");

            GraphProvider.BakeSynchronously();
        }

        protected virtual void AdjustToGround()
        {
            if(GraphProvider.NodeGraph is UnityEngine.Object obj)
                Undo.RegisterCompleteObjectUndo(obj, "Adjust to Ground");

            for (int i = 0; i < GraphProvider.NodeGraph.SerializedNodes.Count; i++)
            {
                var node = GraphProvider.NodeGraph.SerializedNodes[i];

                if (Physics.Raycast(node.position + Vector3.up, Vector3.down, out var hit, float.MaxValue, WorldLayerIndex, QueryTriggerInteraction.Collide))
                {
                    node.position = hit.point + GraphProvider.NodeGraph.NodeOffset;
                }
            }
        }

        protected virtual void Clear()
        {
            if (GraphProvider.NodeGraph is UnityEngine.Object obj)
                Undo.RegisterCompleteObjectUndo(obj, "Clear");

            if (EditorUtility.DisplayDialog("WARNING: Clear All Nodes", "Clicking this button will delete EVERY node. Are you sure you want to do this?", "Yes, Im sure", "No, Take me back"))
            {
                GraphProvider.Clear();
            }
        }

        protected virtual void OnKeyDown(Event current)
        {
            switch (current.keyCode)
            {
                case KeyCode.B:
                    if (GraphProvider.NodeGraph is UnityEngine.Object obj)
                        Undo.RegisterCompleteObjectUndo(obj, "Add Node on Raycast Hit Point");
                    GraphProvider.AddNewNode(HitInfo.point);
                    current.Use();
                    break;
                case KeyCode.N:
                    if (GraphProvider.NodeGraph is UnityEngine.Object obj1)
                        Undo.RegisterCompleteObjectUndo(obj1, "Add Node on Camera Pos");
                    GraphProvider.AddNewNode(Camera.current.transform.position);
                    current.Use();
                    break;
                case KeyCode.M:
                    if (GraphProvider.NodeGraph is UnityEngine.Object obj2)
                        Undo.RegisterCompleteObjectUndo(obj2, "Remove nearest node");
                    GraphProvider.RemoveNearestNode(HitInfo.point);
                    current.Use();
                    break;
            }
        }

        protected virtual void DrawNodes(Event currentEvent)
        {
            if(nodes.Count > 0)
            {
                bool inRange = false;
                foreach(var serializedNode in nodes)
                {
                    var worldPos = LocalToWorldPosition(serializedNode.position);
                    if (math.any(math.isnan(worldPos)) || math.any(math.isinf(worldPos)))
                        continue;

                    if (!HitInfo.collider)
                        continue;

                    var dist = Vector3.Distance(worldPos, HitInfo.point);
                    var dir = HitInfo.point - worldPos;
                    dir = dir.normalized;
                    Ray losRay = new Ray(worldPos, dir);

                    if (Physics.Raycast(ray, out var losHit, dist, WorldLayerIndex))
                    {
                        if (IsWall(losHit, losRay))
                            continue;
                    }

                    float distance = Vector3.Distance(worldPos, HitInfo.point);

                    if(distance <= SerializedPathNode.MAX_DISTANCE * NebulaMath.GetAverage(GraphProviderScale))
                    {
                        Handles.color = Color.yellow;
                        Handles.DrawLine(worldPos, HitInfo.point, 3);
                        inRange = true;
                    }
                }
                Handles.color = inRange ? Color.yellow : Color.red; 
            }
            else
            {
                Handles.color = Color.yellow;
            }

            if(HitInfo.collider)
            {
                Handles.CylinderHandleCap(ControlID, HitInfo.point, Quaternion.Euler(90, 0, 0), 0.5f, EventType.Repaint);
            }

            for(int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var worldPosition = LocalToWorldPosition(node.position);
                if (math.any(math.isnan(worldPosition)) || math.any(math.isinf(worldPosition)))
                    continue;

                Vector3 diff = worldPosition - ray.origin;
                if (diff.sqrMagnitude > 22500)
                    continue;

                Handles.color = node.serializedPathNodeLinkIndices.Count <= 0 ? Color.yellow : Color.green;
                float scale = 0.5f;
                Handles.CylinderHandleCap(ControlID, worldPosition, Quaternion.Euler(90, 0, 0), scale, EventType.Repaint);

                var pos = worldPosition;
                pos.y += 1;
                Handles.color = Color.white;
                Handles.Label(pos, i.ToString(), new GUIStyle(GUI.skin.GetStyle("SettingsHeader")));

                var nodeXZ = new Vector3(worldPosition.x, 0, worldPosition.z);
                var rayOriginXZ = new Vector3(ray.origin.x, 0, ray.origin.z);

                if(!EditorApplication.isPlaying && Vector3.Distance(nodeXZ, rayOriginXZ) < 20)
                {
                    worldPosition = Handles.PositionHandle(worldPosition, Quaternion.identity);
                    node.position = WorldToLocalPosition(worldPosition);
                }

                Handles.color = Color.magenta;

                var links = GraphProvider.GetSerializedPathLinks();
                foreach(var linkIndex in node.serializedPathNodeLinkIndices)
                {
                    var link = links[linkIndex];

                    if (link.nodeBIndex == -1 || link.nodeAIndex == -1)
                        continue;

                    var endNode = nodes[link.nodeBIndex];
                    var endNodePosition = LocalToWorldPosition(endNode.position);

                    if (math.any(math.isnan(endNodePosition)) || math.any(math.isinf(endNodePosition)))
                        continue;

                    Handles.DrawLine(worldPosition, endNodePosition, 5);
                }
            }
        }

        protected virtual Vector3 LocalToWorldPosition(Vector3 localPos)
        {
            if(_transform)
            {
                localPos = _transform.TransformPoint(localPos);
            }
            return localPos;
        }

        protected virtual Vector3 WorldToLocalPosition(Vector3 worldPos)
        {
            if (_transform)
            {
                worldPos = _transform.InverseTransformPoint(worldPos);
            }
            return worldPos;
        }
        protected virtual void DrawSceneGUI(Event currentEvent)
        {
            Handles.BeginGUI();
            if(EditorApplication.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
            }
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
            if (EditorApplication.isPlaying)
            {
                EditorGUI.EndDisabledGroup();
            }
            Handles.EndGUI();
        }

        protected virtual bool IsWall(RaycastHit LineOfSightHit, Ray ray)
        {
            return Vector3.Angle(LineOfSightHit.normal, ray.direction) >= 90;
        }

        public GraphProviderDrawer(IGraphProvider graphProvider, int worldLayerIndex)
        {
            GraphProvider = graphProvider;
            WorldLayerIndex = worldLayerIndex;

            if(graphProvider is MonoBehaviour b)
            {
                _transform = b.transform;
            }
        }
    }
}
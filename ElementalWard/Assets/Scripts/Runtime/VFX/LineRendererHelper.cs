using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class LineRendererHelper : MonoBehaviour
    {
        public Vector3 StartPos 
        {
            get => _startPoint ? _startPoint.position : Vector3.zero;
            set
            {
                if (_startPoint)
                    _startPoint.position = value;
            }
        }
        [SerializeField] private Transform _startPoint;

        public Vector3 EndPos
        {
            get => _endPoint ? _endPoint.position : Vector3.zero;
            set
            {
                if (_endPoint)
                    _endPoint.position = value;
            }
        }
        [SerializeField] private Transform _endPoint;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void LateUpdate()
        {
            _lineRenderer.SetPosition(0, StartPos);
            _lineRenderer.SetPosition(1, EndPos);
        }
    }
}

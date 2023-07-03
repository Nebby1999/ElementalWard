using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalWard
{
    public class BobTransform : MonoBehaviour
    {
        public AnimationCurve xCurve;
        public AnimationCurve yCurve;
        public AnimationCurve zCurve;
        public float bobDuration;

        private float _internalStopwatch;
        private Transform _transform;
        private Vector3 _localPosAtAwake;

        private void Awake()
        {
            _transform = transform;
            _localPosAtAwake = _transform.localPosition;
        }

        private void OnEnable()
        {
            _transform.position = _localPosAtAwake;
            _internalStopwatch = 0;
        }

        private void OnDisable()
        {
            _transform.position = _localPosAtAwake;
        }
        // Update is called once per frame
        void Update()
        {
            _internalStopwatch += Time.deltaTime;
            var num = _internalStopwatch / bobDuration;
            _transform.localPosition = _localPosAtAwake + new Vector3(xCurve.Evaluate(num), yCurve.Evaluate(num), zCurve.Evaluate(num));
        }
    }
}

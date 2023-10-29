using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard.Projectiles
{
    public class ProjectileFuse : MonoBehaviour
    {
        public float fuseDuration;
        public UnityEvent OnFuseEnd;

        private float _fuseStopWatch;

        private void OnEnable()
        {
            _fuseStopWatch = 0;
        }

        private void OnDisable()
        {
            _fuseStopWatch = 0;
        }

        private void FixedUpdate()
        {
            _fuseStopWatch += Time.fixedDeltaTime;
            if(_fuseStopWatch > fuseDuration)
            {
                enabled = false;
                OnFuseEnd?.Invoke();
            }
        }
    }
}
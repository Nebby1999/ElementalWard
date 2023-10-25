using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard.Projectiles
{
    [RequireComponent(typeof(ProjectileTarget))]
    public class ProjectileTargetFinder : MonoBehaviour, IProjectileInitialization
    {
        public float lookRange;
        public float lookCone;
        public float searchInterval;
        public UnityEvent<HurtBox> OnNewTargetfound;
        public UnityEvent OnTargetLost;
        public bool HasTarget => ProjectileTarget.Target;
        public ProjectileTarget ProjectileTarget { get; private set; }

        private new Transform transform;
        private TeamIndex _teamIndex;
        private BullseyeSearch _search = new BullseyeSearch();
        private float _searchTimer;
        private bool _hadTargetLastUpdate;
        private HurtBox _targetHurtBox;
        private Transform _target;

        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            _teamIndex = fireProjectileInfo.owner.team;
            _search.teamFilter = TeamMask.allButNeutral;
            _search.teamFilter.RemoveTeam(_teamIndex);
        }

        private void Awake()
        {
            transform = base.transform;
            _searchTimer = 0f;
            _search.maxDistanceFilter = lookRange;
            _search.SortBy = BullseyeSearch.SortByDistance;
            _search.viewer = gameObject;
            _search.filterByLOS = true;
            
            ProjectileTarget = GetComponent<ProjectileTarget>();
        }

        private void FixedUpdate()
        {
            if (!_target)
                return;

            _searchTimer += Time.fixedDeltaTime;
            if(_searchTimer > searchInterval)
            {
                _searchTimer = 0f;
                SearchForTarget();
            }

            if (_hadTargetLastUpdate != HasTarget)
            {
                if(HasTarget)
                {
                    OnNewTargetfound?.Invoke(_targetHurtBox);
                }
                else
                {
                    OnTargetLost?.Invoke();
                }
            }
            _hadTargetLastUpdate = HasTarget;
        }

        private void SearchForTarget()
        {
            _search.searchOrigin = transform.position;
            _search.searchDirection = transform.forward;
            _search.MaxAngleFilter = lookCone;
            _search.RefreshCandidates();
            HurtBox[] source = _search.GetResults();
            HurtBox target = source.FirstOrDefault();

            _target = target ? target.transform : null;
            _targetHurtBox = target;
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.yellow;
            Vector3 rotatedForward = Quaternion.Euler(0, -lookCone * 0.5f, 0) * base.transform.forward;
            UnityEditor.Handles.DrawWireArc(base.transform.position, Vector3.up, rotatedForward, lookCone, lookRange);
#else
            Gizmos.color = Color.yellow;
		    Transform transform = base.transform;
		    Vector3 position = transform.position;
		    Gizmos.DrawWireSphere(position, lookRange);
		    Gizmos.DrawRay(position, transform.forward * lookRange);
		    Gizmos.DrawFrustum(position, lookCone, lookRange, 0f, 1f);
#endif
        }
    }
}
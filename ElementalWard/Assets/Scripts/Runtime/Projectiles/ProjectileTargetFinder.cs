﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalWard.Projectiles
{
    [RequireComponent(typeof(ProjectileTarget))]
    public class ProjectileTargetFinder : MonoBehaviour, IProjectileInitialization
    {
        public float lookRange;
        [Range(0, 180)]
        public float lookCone;
        public float searchInterval;
        public bool allowTargetLoss;
        public UnityEvent<HurtBox> OnNewTargetFound;
        public UnityEvent OnTargetLost;
        public bool HasTarget => ProjectileTarget.Target;
        public ProjectileTarget ProjectileTarget { get; private set; }

        private new Transform transform;
        private TeamIndex _teamIndex;
        private BullseyeSearch _search = new BullseyeSearch();
        private float _searchTimer;
        private bool _hadTargetLastUpdate;
        private HurtBox _targetHurtBox;

        public void Initialize(FireProjectileInfo fireProjectileInfo)
        {
            _teamIndex = fireProjectileInfo.owner.team;
            _search.teamMaskFilter = TeamMask.allButNeutral;
            _search.teamMaskFilter.RemoveTeam(_teamIndex);
        }

        private void Awake()
        {
            transform = base.transform;
            _searchTimer = 0f;
            _search.maxDistanceFilter = lookRange;
            _search.sortMode = BullseyeSearch.SortMode.Distance;
            _search.viewer = gameObject;
            _search.filterByLoS = true;
            
            ProjectileTarget = GetComponent<ProjectileTarget>();
        }

        private void FixedUpdate()
        {
            if (ProjectileTarget.Target)
            {
                if (!allowTargetLoss)
                    return;

                var distance = Vector3.Distance(ProjectileTarget.Target.position, transform.position);
                if (distance <= lookRange)
                {
                    return;
                }
                ProjectileTarget.Target = null;
            }

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
                    OnNewTargetFound?.Invoke(_targetHurtBox);
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
            HurtBox target = _search.GetResults().FirstOrDefault();

            ProjectileTarget.Target = target ? target.transform : null;
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
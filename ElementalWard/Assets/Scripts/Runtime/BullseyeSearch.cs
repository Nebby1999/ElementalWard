using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using static ElementalWard.BullseyeSearch;

namespace ElementalWard
{
    public class BullseyeSearch
    {

        public GameObject viewer;
        public Vector3 searchOrigin;
        public Vector3 searchDirection;
        public float minDistanceFilter;
        public float maxDistanceFilter = float.PositiveInfinity;
        public bool filterByLOS = true;
        public bool filterByDistinctEntity;
        public SortDelegate SortBy;
        public TeamMask teamFilter;
        public QueryTriggerInteraction queryTriggerInteraction;

        public float MinAngleFilter
        {
            get => _minAngleFilter;
            set
            {
                _minAngleFilter = value;
                var thetaDot = Mathf.Cos(Mathf.Clamp(value, 0f, 180f) * ((float)Mathf.PI / 180f));
                _maxThetaDot = thetaDot;
            }
        }
        private float _minAngleFilter;
        private float _maxThetaDot = 1f;

        public float MaxAngleFilter
        {
            get => _maxAngleFilter;
            set
            {
                _maxAngleFilter = value;
                var thetaDot = Mathf.Cos(Mathf.Clamp(value, 0f, 180f) * ((float)Mathf.PI / 180f));
                _minThetaDot = thetaDot;
            }
        }
        private float _maxAngleFilter;
        private float _minThetaDot = -1f;

        private bool FilterByDistance
        {
            get
            {
                if(!(minDistanceFilter > 0f) && !(maxDistanceFilter < float.PositiveInfinity))
                {
                    return false;
                }
                return true;
            }
        }
        private bool FilterByAngle
        {
            get
            {
                if (!(_minThetaDot > -1f))
                {
                    return _maxThetaDot < 1f;
                }
                return true;
            }
        }
        private IEnumerable<CandidateInfo> _candidates;


        public void RefreshCandidates()
        {
            var selector = GetSelector();
            _candidates = HurtBox.EnabledBullseyeHurtBoxes.Where(h => teamFilter.HasTeam(h.TeamIndex)).Select(selector);

            if(FilterByAngle)
            {
                _candidates = _candidates.Where(DotOkay);
            }

            if(FilterByDistance)
            {
                _candidates = _candidates.Where(DistanceOkay);
            }

            if(filterByDistinctEntity)
            {
                _candidates = _candidates.Distinct(default(CandidateInfo.EntityEqualityComparer));
            }

            var sorter = SortBy;
            if (sorter != null)
            {
                _candidates = _candidates.OrderBy((x) => sorter(x));
            }
        }

        private bool DotOkay(CandidateInfo info)
        {
            if (_minThetaDot <= info.dot)
            {
                return info.dot <= _maxThetaDot;
            }
            return false;
        }

        private bool DistanceOkay(CandidateInfo info)
        {
            float minDistanceSqr = Mathf.Pow(minDistanceFilter, 2);
            float maxDistanceSqr = Mathf.Pow(maxDistanceFilter, 2);
            if (info.distanceSqr >= minDistanceSqr)
            {
                return info.distanceSqr <= maxDistanceSqr;
            }
            return false;
        }
        public HurtBox[] GetResults()
        {
            if(filterByLOS)
            {
                _candidates = _candidates.Where(c => CheckLOS(c.position));
            }
            return _candidates.Select(c => c.hurtBox).ToArray();
        }

        private bool CheckLOS(Vector3 position)
        {
            Vector3 direction = position - searchOrigin;
            return !Physics.Raycast(searchOrigin, direction, out _, direction.magnitude, LayerIndex.world.Mask, queryTriggerInteraction);
        }

        public Func<HurtBox, CandidateInfo> GetSelector()
        {
            return (HurtBox h) =>
            {
                CandidateInfo info = default;
                info.hurtBox = h;
                info.position = h.transform.position;
                Vector3 difference = info.position - searchOrigin;
                info.dot = Vector3.Dot(searchDirection, difference.normalized);
                info.distanceSqr = difference.sqrMagnitude;
                return info;
            };
        }
        public static float SortByDistance(CandidateInfo info) => info.distanceSqr;
        public static float SortByAngle(CandidateInfo info) => info.dot;
        public static float SortByDistanceAndAngle(CandidateInfo info) => (0f - info.dot) * info.distanceSqr;
        public delegate float SortDelegate(CandidateInfo info);
        public struct CandidateInfo
        {
            [StructLayout(LayoutKind.Sequential, Size = 1)]
            public struct EntityEqualityComparer : IEqualityComparer<CandidateInfo>
            {
                public bool Equals(CandidateInfo a, CandidateInfo b)
                {
                    return (object)a.hurtBox.HealthComponent == b.hurtBox.HealthComponent;
                }

                public int GetHashCode(CandidateInfo obj)
                {
                    return obj.hurtBox.HealthComponent.GetHashCode();
                }
            }

            public HurtBox hurtBox;

            public Vector3 position;

            public float dot;

            public float distanceSqr;
        }
    }
}
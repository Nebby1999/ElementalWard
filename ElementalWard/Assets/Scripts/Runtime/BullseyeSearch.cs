
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ElementalWard;
using UnityEngine;

public class BullseyeSearch
{
    private struct CandidateInfo
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

    public enum SortMode
    {
        None = 0,
        Distance = 1,
        Angle = 2,
        DistanceAndAngle = 3
    }

    private delegate CandidateInfo Selector(HurtBox hurtBox);

    public GameObject viewer;

    public Vector3 searchOrigin;

    public Vector3 searchDirection;

    private float minThetaDot = -1f;

    private float maxThetaDot = 1f;

    public float minDistanceFilter;

    public float maxDistanceFilter = float.PositiveInfinity;

    public TeamMask teamMaskFilter = TeamMask.allButNeutral;

    public bool filterByLoS = true;

    public bool filterByDistinctEntity;

    public QueryTriggerInteraction queryTriggerInteraction;

    public SortMode sortMode = SortMode.Distance;

    private IEnumerable<CandidateInfo> candidatesEnumerable;

    public float MinAngleFilter
    {
        set
        {
            maxThetaDot = Mathf.Cos(Mathf.Clamp(value, 0f, 180f) * ((float)Math.PI / 180f));
        }
    }

    public float MaxAngleFilter
    {
        set
        {
            minThetaDot = Mathf.Cos(Mathf.Clamp(value, 0f, 180f) * ((float)Math.PI / 180f));
        }
    }

    private bool FilterByDistance
    {
        get
        {
            if (!(minDistanceFilter > 0f) && !(maxDistanceFilter < float.PositiveInfinity))
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
            if (!(minThetaDot > -1f))
            {
                return maxThetaDot < 1f;
            }
            return true;
        }
    }

    private Func<HurtBox, CandidateInfo> GetSelector()
    {
        bool getDot = FilterByAngle;
        bool getDistanceSqr = FilterByDistance;
        getDistanceSqr |= sortMode == SortMode.Distance || sortMode == SortMode.DistanceAndAngle;
        getDot |= sortMode == SortMode.Angle || sortMode == SortMode.DistanceAndAngle;
        bool getDifference = getDot || getDistanceSqr;
        bool getPosition = getDot || getDistanceSqr || filterByLoS;
        return delegate (HurtBox hurtBox)
        {
            CandidateInfo candidateInfo = default;
            candidateInfo.hurtBox = hurtBox;
            CandidateInfo result = candidateInfo;
            if (getPosition)
            {
                result.position = hurtBox.transform.position;
            }
            Vector3 vector = default;
            if (getDifference)
            {
                vector = result.position - searchOrigin;
            }
            if (getDot)
            {
                result.dot = Vector3.Dot(searchDirection, vector.normalized);
            }
            if (getDistanceSqr)
            {
                result.distanceSqr = vector.sqrMagnitude;
            }
            return result;
        };
    }

    public void RefreshCandidates()
    {
        Func<HurtBox, CandidateInfo> selector = GetSelector();
        candidatesEnumerable = HurtBox.EnabledBullseyeHurtBoxes.Where((HurtBox hurtBox) => teamMaskFilter.HasTeam(hurtBox.TeamIndex)).Select(selector);
        if (FilterByAngle)
        {
            candidatesEnumerable = candidatesEnumerable.Where(DotOkay);
        }
        float minDistanceSqr;
        float maxDistanceSqr;
        if (FilterByDistance)
        {
            float num = maxDistanceFilter;
            minDistanceSqr = minDistanceFilter * minDistanceFilter;
            maxDistanceSqr = num * num;
            candidatesEnumerable = candidatesEnumerable.Where(DistanceOkay);
        }
        if (filterByDistinctEntity)
        {
            candidatesEnumerable = candidatesEnumerable.Distinct(default(CandidateInfo.EntityEqualityComparer));
        }
        Func<CandidateInfo, float> sorter = GetSorter();
        if (sorter != null)
        {
            candidatesEnumerable = candidatesEnumerable.OrderBy(sorter);
        }
        bool DistanceOkay(CandidateInfo candidateInfo)
        {
            if (candidateInfo.distanceSqr >= minDistanceSqr)
            {
                return candidateInfo.distanceSqr <= maxDistanceSqr;
            }
            return false;
        }
        bool DotOkay(CandidateInfo candidateInfo)
        {
            if (minThetaDot <= candidateInfo.dot)
            {
                return candidateInfo.dot <= maxThetaDot;
            }
            return false;
        }
    }

    private Func<CandidateInfo, float> GetSorter()
    {
        return sortMode switch
        {
            SortMode.Distance => (CandidateInfo candidateInfo) => candidateInfo.distanceSqr,
            SortMode.Angle => (CandidateInfo candidateInfo) => 0f - candidateInfo.dot,
            SortMode.DistanceAndAngle => (CandidateInfo candidateInfo) => (0f - candidateInfo.dot) * candidateInfo.distanceSqr,
            _ => null,
        };
    }

    public void FilterOutGameObject(GameObject gameObject)
    {
        candidatesEnumerable = candidatesEnumerable.Where((CandidateInfo v) => v.hurtBox.HealthComponent.gameObject != gameObject);
    }

    public IEnumerable<HurtBox> GetResults()
    {
        IEnumerable<CandidateInfo> source = candidatesEnumerable;
        if (filterByLoS)
        {
            source = source.Where((CandidateInfo candidateInfo) => CheckLoS(candidateInfo.position));
        }
        if ((bool)viewer)
        {
            //source = source.Where((CandidateInfo candidateInfo) => CheckVisible(candidateInfo.hurtBox.HealthComponent.gameObject));
        }
        return source.Select((CandidateInfo candidateInfo) => candidateInfo.hurtBox);
    }

    private bool CheckLoS(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - searchOrigin;
        RaycastHit hitInfo;
        return !Physics.Raycast(searchOrigin, direction, out hitInfo, direction.magnitude, LayerIndex.world.Mask, queryTriggerInteraction);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using static ElementalWard.SphereSearch;

namespace ElementalWard
{
    public class SphereSearch
    {
        public float radius;
        public Vector3 origin;
        public LayerMask candidateMask;
        public QueryTriggerInteraction triggerInteraction;

        private List<Candidate> candidates;
        public SphereSearch FindCandidates()
        {
            candidates = new List<Candidate>();
            var colliders = Physics.OverlapSphere(origin, radius, candidateMask, triggerInteraction);
            foreach(var collider in colliders)
            {
                var teamProvider = collider.GetComponent<ITeamProvider>();
                candidates.Add(new Candidate
                {
                    collider = collider,
                    team = teamProvider?.TeamIndex ?? TeamIndex.None,
                    distanceSqr = (origin - collider.transform.position).sqrMagnitude,
                    hurtBox = collider.GetComponent<HurtBox>(),
                    position = collider.transform.position,
                });
            }
            return this;
        }

        public SphereSearch FilterCandidatesByDistinctHealthComponent()
        {
            if (candidates == null)
                throw new NullReferenceException("Candidate List not made, call FindCandidates first.");

            List<HealthComponent> distinct = new List<HealthComponent>();
            for(int i = candidates.Count - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                if(!candidate.hurtBox)
                {
                    candidates.RemoveAt(i);
                    continue;
                }

                var healthComponent = candidate.hurtBox.HealthComponent;
                if(!healthComponent)
                {
                    candidates.RemoveAt(i);
                    continue;
                }

                if(distinct.Contains(healthComponent))
                {
                    candidates.RemoveAt(i);
                    continue;
                }
                distinct.Add(healthComponent);
            }
            return this;
        }

        public SphereSearch FilterCandidatesByTeam(TeamMask teamMask)
        {
            ThrowIfCandidateListNull();
            for(int i = candidates.Count - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                if(!teamMask.HasTeam(candidate.team))
                {
                    candidates.RemoveAt(i);
                    continue;
                }
            }
            return this;
        }

        public SphereSearch FilterCandidatesByLOS(LayerMask obstacleMask)
        {
            ThrowIfCandidateListNull();
            int count = candidates.Count;
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(count, Allocator.TempJob);
            var queryParams = new QueryParameters(obstacleMask);
            for (int i = 0; i < count; i++)
            {
                var candidate = candidates[i];
                commands[i] = new RaycastCommand(origin, (origin - candidate.position).normalized, queryParams, Mathf.Sqrt(candidate.distanceSqr));
            }
            RaycastCommand.ScheduleBatch(commands, hits, 1).Complete();
            for(int i = count - 1; i >= 0; i--)
            {
                if (hits[i].collider)
                {
                    candidates.RemoveAt(i);
                }
            }
            commands.Dispose();
            hits.Dispose();
            return this;
        }

        public SphereSearch FilterBy(Func<Candidate, bool> predicate)
        {
            ThrowIfCandidateListNull();
            for(int i = candidates.Count - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                if(!predicate(candidate))
                {
                    candidates.RemoveAt(i);
                }
            }
            return this;
        }

        public SphereSearch OrderByDistance()
        {
            ThrowIfCandidateListNull();
            candidates = candidates.OrderBy(k => k.distanceSqr).ToList();
            return this;
        }

        public SphereSearch GetResults(out List<Candidate> results)
        {
            ThrowIfCandidateListNull();
            results = new List<Candidate>();
            for(int i = 0; i < candidates.Count; i++)
            {
                results.Add(candidates[i]);
            }
            candidates.Clear();
            return this;
        }

        private void ThrowIfCandidateListNull()
        {
            if (candidates == null)
                throw new NullReferenceException("Candidate List not made, call FindCandidates first.");
        }

        public SphereSearch FirstOrDefault(out Candidate candidate)
        {
            candidate = candidates.FirstOrDefault();
            return this;
        }

        public struct Candidate
        {
            public Collider collider;
            public HurtBox? hurtBox;
            public TeamIndex team;
            public Vector3 position;
            public float distanceSqr;
        }
    }
}
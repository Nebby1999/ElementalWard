using UnityEngine;

namespace ElementalWard
{
    public class DisableParticleLooping : MonoBehaviour
    {
        public bool changeDuration;
        public float newDuration;
        public ParticleSystem[] systems;

        public void DisableLooping()
        {
            foreach (ParticleSystem system in systems)
            {
                var main = system.main;
                if (changeDuration)
                {
                    system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    main.duration = newDuration;
                    main.startLifetime = newDuration;
                    main.prewarm = true;
                    system.Play(true);
                }
                main.loop = false;
            }
        }
    }
}

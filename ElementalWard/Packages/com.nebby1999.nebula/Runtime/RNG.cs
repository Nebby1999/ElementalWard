using R = UnityEngine.Random;
namespace Nebula
{
    public class RNG
    {
        private R.State state;

        public RNG(int seed)
        {
            R.InitState(seed);
            var _ = R.value;
            state = R.state;
        }
    }
}
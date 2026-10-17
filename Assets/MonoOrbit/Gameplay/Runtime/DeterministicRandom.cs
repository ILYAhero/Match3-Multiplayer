using System;

namespace MonoOrbit.Gameplay
{
    public interface IRandomSource
    {
        int Next(int exclusiveMax);
    }

    /// <summary>
    /// Fixed xorshift32 sequence independent of UnityEngine.Random.
    /// The host owns its seed/state; clients receive authoritative results.
    /// </summary>
    public sealed class DeterministicRandom : IRandomSource
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0 ? 0x9E3779B9u : seed;
        }

        public int Next(int exclusiveMax)
        {
            if (exclusiveMax <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return (int)(x % (uint)exclusiveMax);
        }
    }
}

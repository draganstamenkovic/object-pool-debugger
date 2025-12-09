using System.Collections.Generic;
using UnityEngine;

namespace ObjectPoolDebugger.Runtime
{
	public class PoolDebuggerRuntime
    {
        // Singleton instance (lightweight)
        private static PoolDebuggerRuntime _instance;
        public static PoolDebuggerRuntime Instance => _instance ??= new PoolDebuggerRuntime();

        private readonly Dictionary<string, PoolStats> _pools = new();

        public IReadOnlyDictionary<string, PoolStats> Pools => _pools;

        // Called when a pool is created/registered
        public void NotifyPoolCreated(string poolId, int initialInactive)
        {
            if (!_pools.ContainsKey(poolId))
            {
                var s = new PoolStats { PoolId = poolId, InactiveCount = initialInactive };
                _pools[poolId] = s;
            }
            else
            {
                var existing = _pools[poolId];
                existing.InactiveCount = initialInactive;
            }
        }

        public void NotifySpawn(string poolId, GameObject obj)
        {
            if (!_pools.TryGetValue(poolId, out var s))
            {
                s = new PoolStats { PoolId = poolId };
                _pools[poolId] = s;
            }

            s.RecordSpawn(obj);
        }

        public void NotifyDespawn(string poolId, GameObject obj)
        {
            if (!_pools.TryGetValue(poolId, out var s))
            {
                s = new PoolStats { PoolId = poolId };
                _pools[poolId] = s;
            }

            s.RecordDespawn(obj);
        }

        public void NotifyResize(string poolId, int newInactive)
        {
            if (!_pools.TryGetValue(poolId, out var s))
            {
                s = new PoolStats { PoolId = poolId };
                _pools[poolId] = s;
            }

            s.InactiveCount = newInactive;
            s.RecordInactiveIncreased();
        }

        // Should be called periodically (e.g. every 1 s) - Editor window does this.
        public void SampleAllRates()
        {
            foreach (var kv in _pools)
            {
                kv.Value.SampleRates();
            }
        }
    }
}
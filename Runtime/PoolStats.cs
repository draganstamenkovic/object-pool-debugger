using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectPoolDebugger.Runtime
{
    [Serializable]
    public class PoolStats
    {
        public string PoolId;
        public int ActiveCount;
        public int InactiveCount;
        public int TotalCount => ActiveCount + InactiveCount;
        public int MaxSizeObserved;
        public float SpawnsPerSec;
        public float DespawnsPerSec;

        // Internal counters for short-term rate calculations
        internal int _spawnCounter;
        internal int _despawnCounter;
        internal float _lastRateSampleTime;
        public Queue<(float time, string evt)> Timeline = new();

        public void RecordSpawn(GameObject obj)
        {
            ActiveCount++;
            _spawnCounter++;
            Timeline.Enqueue((Time.realtimeSinceStartup, "spawn"));
            MaxSizeObserved = Math.Max(MaxSizeObserved, TotalCount);
            TrimTimeline();
        }

        public void RecordDespawn(GameObject obj)
        {
            ActiveCount = Math.Max(0, ActiveCount - 1);
            _despawnCounter++;
            Timeline.Enqueue((Time.realtimeSinceStartup, "despawn"));
            TrimTimeline();
        }

        public void RecordInactiveIncreased()
        {
            InactiveCount++;
            MaxSizeObserved = Math.Max(MaxSizeObserved, TotalCount);
        }

        public void RecordInactiveDecreased()
        {
            InactiveCount = Math.Max(0, InactiveCount - 1);
        }

        public void SampleRates()
        {
            var t = Time.realtimeSinceStartup;
            var dt = Mathf.Max(0.0001f, t - _lastRateSampleTime);
            if (_lastRateSampleTime <= 0)
            {
                _lastRateSampleTime = t;
                _spawnCounter = 0;
                _despawnCounter = 0;
                return;
            }

            SpawnsPerSec = _spawnCounter / dt;
            DespawnsPerSec = _despawnCounter / dt;
            _lastRateSampleTime = t;
            _spawnCounter = 0;
            _despawnCounter = 0;
        }

        void TrimTimeline()
        {
            // keep last ~200 events to avoid memory growth
            while (Timeline.Count > 200) Timeline.Dequeue();
        }
    }
}
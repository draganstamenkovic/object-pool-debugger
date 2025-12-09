using UnityEngine;

namespace ObjectPoolDebugger.Runtime
{
    // Simple component that marks an instance as pooled and stores pool ID
    public class PooledObject : MonoBehaviour
    {
        public string PoolId;
        internal bool IsPooledInstance;
    }
}
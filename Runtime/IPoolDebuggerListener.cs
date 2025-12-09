namespace ObjectPoolDebugger.Runtime
{
    public interface IPoolDebuggerListener
    {
        void OnPoolCreated(string poolId, int initialSize);
        void OnSpawn(string poolId, UnityEngine.GameObject obj);
        void OnDespawn(string poolId, UnityEngine.GameObject obj);
        void OnResize(string poolId, int newSize);
    }
}
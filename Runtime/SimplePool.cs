using System.Collections.Generic;
using UnityEngine;

namespace ObjectPoolDebugger.Runtime
{
	public class SimplePool : MonoBehaviour
    {
        [Tooltip("Unique ID for this pool (e.g. 'BulletPool')")]
        public string PoolId = "SimplePool";
        public GameObject Prefab;
        public int InitialSize = 10;
        public bool ExpandIfEmpty = true;

        private readonly Queue<GameObject> _queue = new();
        private readonly HashSet<GameObject> _active = new();

        void Awake()
        {
            if (string.IsNullOrEmpty(PoolId)) PoolId = $"{gameObject.name}_Pool";
            for (int i = 0; i < InitialSize; i++)
            {
                var go = CreateInstance();
                go.SetActive(false);
                _queue.Enqueue(go);
            }

            PoolDebuggerRuntime.Instance.NotifyPoolCreated(PoolId, _queue.Count);
        }

        GameObject CreateInstance()
        {
            var go = Instantiate(Prefab, transform);
            var po = go.GetComponent<PooledObject>();
            if (po == null) po = go.AddComponent<PooledObject>();
            po.PoolId = PoolId;
            po.IsPooledInstance = true;
            return go;
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject go;
            if (_queue.Count > 0)
            {
                go = _queue.Dequeue();
                go.transform.SetPositionAndRotation(position, rotation);
            }
            else if (ExpandIfEmpty)
            {
                go = CreateInstance();
                PoolDebuggerRuntime.Instance.NotifyResize(PoolId, _queue.Count);
            }
            else
            {
                return null;
            }

            go.SetActive(true);
            _active.Add(go);
            PoolDebuggerRuntime.Instance.NotifySpawn(PoolId, go);
            return go;
        }

        public void Despawn(GameObject go)
        {
            if (go == null) return;
            if (!_active.Contains(go))
            {
                // If it's not tracked as active, still enqueue to avoid leak
                if (!go.GetComponent<PooledObject>())
                    go.AddComponent<PooledObject>().PoolId = PoolId;
            }
            else
            {
                _active.Remove(go);
            }

            go.SetActive(false);
            _queue.Enqueue(go);
            PoolDebuggerRuntime.Instance.NotifyDespawn(PoolId, go);
        }

        // Convenience: despawn all
        public void DespawnAll()
        {
            foreach (var a in new List<GameObject>(_active))
            {
                Despawn(a);
            }
        }

        public int ActiveCount => _active.Count;
        public int InactiveCount => _queue.Count;
    }
}
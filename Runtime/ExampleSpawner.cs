using UnityEngine;

namespace ObjectPoolDebugger.Runtime
{
    [AddComponentMenu("ObjectPoolDebugger/ExampleSpawner")]
    public class ExampleSpawner : MonoBehaviour
    {
        public SimplePool Pool;
        public float SpawnInterval = 0.2f;
        public float DespawnAfter = 2f;

        float _timer;
        void Update()
        {
            if (Pool == null) return;
            _timer += Time.deltaTime;
            if (_timer >= SpawnInterval)
            {
                _timer = 0f;
                var obj = Pool.Spawn(transform.position + Random.insideUnitSphere * 2f, Quaternion.identity);
                if (obj)
                    StartCoroutine(DespawnRoutine(obj));
            }
        }

        System.Collections.IEnumerator DespawnRoutine(GameObject obj)
        {
            yield return new WaitForSeconds(DespawnAfter);
            if (obj != null)
                Pool.Despawn(obj);
        }
    }
}
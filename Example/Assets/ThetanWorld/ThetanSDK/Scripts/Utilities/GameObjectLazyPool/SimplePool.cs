using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.Pooling
{
    public partial class SimplePool : MonoBehaviour
    {
        [SerializeField] private Transform _poolParent;

        private Dictionary<int, HashSet<GameObject>> _pool = new Dictionary<int, HashSet<GameObject>>();
        private Dictionary<int, HashSet<GameObject>> _poolTrackingInstance = new Dictionary<int, HashSet<GameObject>>();

        public static SimplePool Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (_poolParent == null)
                _poolParent = transform;
        }

        public GameObject Rent(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"SimplePool: Renting Prefab {prefab.name} is null.");
                return null;
            }

            var prefabID = prefab.GetInstanceID();

            if (!_pool.ContainsKey(prefabID) || _pool[prefabID] == null)
                _pool[prefabID] = new HashSet<GameObject>();

            GameObject instance = null;
            foreach (var obj in _pool[prefabID])
            {
                if (obj.activeSelf)
                    continue;

                instance = obj;
                break;
            }

            if (instance == null)
                instance = Instantiate(prefab, parent);
            else
            {
                instance.transform.SetParent(parent);
                _pool[prefabID].Remove(instance);
            }

            _poolTrackingInstance[instance.GetInstanceID()] = _pool[prefabID];
            instance.SetActive(false);

            return instance;
        }

        public void Return(GameObject instance)
        {
            if (!instance)
                return;

            var instanceID = instance.GetInstanceID();
            if (_poolTrackingInstance.TryGetValue(instanceID, out var pool))
            {
                _poolTrackingInstance.Remove(instanceID);
                instance.SetActive(false);
                instance.transform.SetParent(_poolParent);
                pool.Add(instance);
            }
            else
                Debug.LogWarning(
                    $"SimplePool: GameObject {instance.name} is not registered in the pool or was already returned.");
        }
    }
}

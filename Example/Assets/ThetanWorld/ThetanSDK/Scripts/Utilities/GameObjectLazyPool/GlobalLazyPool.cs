using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Foundation.Pooling.UnityPools;

namespace ThetanSDK.Utilities.Pooling
{
    public class GlobalLazyPool : MonoBehaviour
    {
        private static GlobalLazyPool _instance;

        [SerializeField] private Transform _poolParent;
        
        private MultipleGameObjectPool _multipleGameObjectPool;
        
        private void Awake()
        {
            _instance = this;

            _multipleGameObjectPool = new MultipleGameObjectPool(_poolParent);
        }

        private void OnDestroy()
        {
            if(_instance == this)
                _instance = null;
        }

        public static async UniTask<GameObject> Rent(GameObject gameObjectReference)
        {
            if (_instance == null ||
                _instance._multipleGameObjectPool == null)
                return null;

            return await _instance._multipleGameObjectPool.Rent(gameObjectReference);
        }
        
        public static async UniTask<GameObject> Rent(GameObjectPrefab gameObjectReference)
        {
            if (_instance == null ||
                _instance._multipleGameObjectPool == null)
                return null;

            return await _instance._multipleGameObjectPool.Rent(gameObjectReference);
        }

        public static void Return(GameObject gameObject)
        {
            if (_instance == null ||
                _instance._multipleGameObjectPool == null)
                return;
            
            _instance._multipleGameObjectPool.Return(gameObject);
        }
        
        public void Return(GameObjectPrefab gameObjectReference, GameObject gameObject)
        {
            if (_instance == null ||
                _instance._multipleGameObjectPool == null)
                return;
            
            _instance._multipleGameObjectPool.Return(gameObjectReference, gameObject);
        }

        public void ReleaseInstances(int keep, Action<GameObject> onReleased = null)
        {
            if (_instance == null ||
                _instance._multipleGameObjectPool == null)
                return;
            
            _instance._multipleGameObjectPool.ReleaseInstances(keep, onReleased);
        }
    }
}
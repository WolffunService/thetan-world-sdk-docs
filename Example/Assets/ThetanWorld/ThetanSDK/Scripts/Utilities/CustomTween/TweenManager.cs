using System;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.MultiPlayer;

namespace Wolffun.Tweening
{
    public class TweenManager : MonoBehaviour
    {
        private static List<TweenerCore> _listActiveTweener = new List<TweenerCore>();
        private static Dictionary<Type, TweenInstanceCache> _listPool = new Dictionary<Type, TweenInstanceCache>();
        private static List<TweenerCore> _listKill = new List<TweenerCore>();
        private static List<TweenerCore> _listAdd = new List<TweenerCore>();
        private static List<TweenerCore> _listRemoving = new List<TweenerCore>();
        private static bool _isUpdating;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnInitializaOnLoadMethod()
        {
            _listActiveTweener = new List<TweenerCore>();
            _listPool = new Dictionary<Type, TweenInstanceCache>();
            _listKill = new List<TweenerCore>();
            _isUpdating = false;
        }
        
        public static void RegisterTween(TweenerCore tweener)
        {
            if (_isUpdating)
            {
                if (!_listAdd.Contains(tweener))
                    _listAdd.Add(tweener);
                return;
            }
            if(!_listActiveTweener.Contains(tweener))
                _listActiveTweener.Add(tweener);
        }

        public static void Complete(TweenerCore tweener)
        {
            if (tweener.IsComplete || !tweener.IsActive || !tweener.IsStarted)
                return;
            
            tweener.UpdateWithDeltaTime(tweener.Duration - tweener.CurrentTime);

            if (_isUpdating)
            {
                _listKill.Add(tweener);
                return;
            }
            ReturnTween(tweener.GetType(), tweener);
            _listActiveTweener.Remove(tweener);
        }

        public static void Kill(TweenerCore tweener)
        {
            if (tweener.IsComplete || !tweener.IsActive || !tweener.IsStarted)
                return;
            
            tweener.ResetValue();

            if (_isUpdating)
            {
                _listKill.Add(tweener);
                return;
            }
            ReturnTween(tweener.GetType(), tweener);
            _listActiveTweener.Remove(tweener);
        }

        public static T RentTween<T>() where T : TweenerCore, new()
        {
            var typeT = typeof(T);
            if (_listPool.TryGetValue(typeT, out var pool))
            {
                var tween = pool.Rent() as T;
                tween.IsActive = true;
                return tween;
            }

            var newPool = new TweenInstanceCache<T>();
            _listPool[typeT] = newPool;

            var newTween = newPool.Rent() as T;
            newTween.IsActive = true;
            return newTween;
        }
        
        public static void AddActiveTweenToSequence(TweenerCore tweener)
        {
            if (_isUpdating)
            {
                if (!_listRemoving.Contains(tweener))
                    _listRemoving.Add(tweener);
                return;
            }
            
            _listActiveTweener.Remove(tweener);
            
        }

        private void Update()
        {
            if (_listActiveTweener.Count == 0)
                return;

            TweenManager._isUpdating = true;

            if (_listRemoving.Count > 0)
            {
                foreach (var tweener in _listRemoving)
                {
                    _listActiveTweener.Remove(tweener);
                }
                
                _listRemoving.Clear();
            }
            
            foreach (var tween in _listActiveTweener)
            {
                tween.UpdateWithDeltaTime(tween.IsIndependentTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
            }

            for (int i = _listActiveTweener.Count - 1; i >= 0; i--)
            {
                var tweener = _listActiveTweener[i];

                if (tweener.IsComplete)
                {
                    ReturnTween(tweener.GetType(), _listActiveTweener[i]);
                    _listActiveTweener.RemoveAt(i);
                }
            }
            
            TweenManager._isUpdating = false;

            if (_listKill.Count > 0)
            {
                foreach (var tween in _listKill)
                {
                    ReturnTween(tween.GetType(), tween);
                    _listActiveTweener.Remove(tween);
                }
                _listKill.Clear();
            }

            if (_listAdd.Count > 0)
            {
                foreach (var tween in _listAdd)
                {
                    _listActiveTweener.Add(tween);
                }
                _listAdd.Clear();
            }
        }

        public static void ReturnTween(Type type, TweenerCore tween)
        {
            tween.IsActive = false;
            if(_listPool.TryGetValue(type, out var pool))
                pool.Return(tween);
        }
    }
}
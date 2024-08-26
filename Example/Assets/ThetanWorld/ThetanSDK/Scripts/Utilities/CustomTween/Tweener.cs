using System;
using UnityEngine;

namespace Wolffun.Tweening
{
    public abstract class Tweener : TweenerCore
    {
        protected Action _onCompleteCallback;
        protected float _duration;
        protected bool _isComplete;
        protected bool _isIndependentTimeScale;
        protected float _delay;
        protected Ease _ease;

        private bool _isStarted;
        
        private float _currentTime;
        public override bool IsStarted => _isStarted;
        public override bool IsComplete => _isComplete;
        public override float CurrentTime => _currentTime;
        public override float Duration => _duration;
        public float Delay => _delay;
        public override bool IsIndependentTimeScale => _isIndependentTimeScale;
        

        public override void ResetValue()
        {
            _onCompleteCallback = null;
            _duration = 0;
            _isComplete = false;
            _isIndependentTimeScale = true;
            _ease = Ease.Unset;
            _currentTime = 0;
            _isStarted = false;
        }

        public Tweener SetDuration(float duration)
        {
            _duration = duration;

            return this;
        }

        public Tweener SetDelay(float delay)
        {
            _delay = delay;

            return this;
        }

        public Tweener SetUpdate(bool isIndependentTimescale)
        {
            _isIndependentTimeScale = isIndependentTimescale;

            return this;
        }

        public void OnComplete(Action callback)
        {
            _onCompleteCallback = callback;
        }

        public Tweener SetEase(Ease ease)
        {
            _ease = ease;
            return this;
        }
        
        public override void UpdateWithDeltaTime(float dt)
        {
            if (!_isStarted)
            {
                OnStartTween();
                _isStarted = true;
            }
            _currentTime += dt;
            _currentTime = Mathf.Min(_currentTime, _duration + _delay);

            if (_currentTime < _delay)
                return;

            if (_duration <= 0)
            {
                InternalUpdateValue(1);
                _isComplete = true;
                
                _onCompleteCallback?.Invoke();
                return;
            }
            
            var deltaChanged = EaseEvaluation.Evaluate(_ease, _currentTime - _delay, _duration);

            InternalUpdateValue(deltaChanged);

            if (_currentTime >= _duration + _delay)
            {
                _isComplete = true;
                
                _onCompleteCallback?.Invoke();
            }
        }
        
        protected abstract void InternalUpdateValue(float deltaChange);
        
        protected virtual void OnStartTween(){}
    }
}
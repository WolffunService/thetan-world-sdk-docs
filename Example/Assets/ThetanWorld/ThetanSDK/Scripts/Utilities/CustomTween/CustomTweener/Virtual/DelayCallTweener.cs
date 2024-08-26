using System;

namespace Wolffun.Tweening
{
    public sealed class DelayCallTweener : TweenerCore
    {
        private float _currentTime;
        private float _duration;
        private Action _callback;
        private bool _isComplete;
        private bool _isIndependentTimeScale;
        private bool _isStarted;

        public override bool IsStarted => _isStarted;
        public override bool IsComplete => _isComplete;
        public override float Duration => _duration;
        public override float CurrentTime => _currentTime;
        public override bool IsIndependentTimeScale => _isIndependentTimeScale;
        
        public override void ResetValue()
        {
            _isComplete = false;
            _currentTime = 0;
            _duration = 0;
            _callback = null;
            _isIndependentTimeScale = false;
            _isStarted = false;
        }

        public void SetUp(float duration, Action callback, bool isDependentTimeScale = false)
        {
            _duration = duration;
            _callback = callback;
            _isIndependentTimeScale = isDependentTimeScale;
            _isComplete = false;
            _currentTime = 0;
        }
        
        public override void UpdateWithDeltaTime(float dt)
        {
            if (!_isStarted)
                _isStarted = true;
            
            _currentTime += dt;

            if (_currentTime >= _duration)
            {
                _isComplete = true;
                _callback?.Invoke();
            }
        }
    }
}
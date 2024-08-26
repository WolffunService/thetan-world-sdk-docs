using UnityEngine;

namespace Wolffun.Tweening
{
    public abstract class Vector2Tweener : Tweener
    {
        private Vector2 _startValue;
        private Vector2 _endValue;
        private Vector2 _changedValue;

        protected void SetUp(Vector2 endValue, float duration)
        {
            _endValue = endValue;
            SetDuration(duration);
        }
        
        protected override void OnStartTween()
        {
            base.OnStartTween();

            _startValue = GetCurrentValue();
            _changedValue = _endValue - _startValue;
        }

        protected override void InternalUpdateValue(float deltaChange)
        {
            var valueChanged = _changedValue * deltaChange;

            SetValue(_startValue + valueChanged);
        }

        protected abstract Vector2 GetCurrentValue();
        
        protected abstract void SetValue(Vector2 currentValue);
    }
}
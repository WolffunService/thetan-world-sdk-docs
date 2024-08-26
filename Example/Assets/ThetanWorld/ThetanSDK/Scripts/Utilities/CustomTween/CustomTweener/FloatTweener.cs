namespace Wolffun.Tweening
{
    public abstract class FloatTweener : Tweener
    {
        private float _startValue;
        private float _endValue;
        private float _changeValue;
        
        
        protected void SetUp(float endValue, float duration)
        {
            _endValue = endValue;

            SetDuration(duration);
        }

        protected override void OnStartTween()
        {
            base.OnStartTween();

            _startValue = GetCurrentValue();
            _changeValue = _endValue - _startValue;
        }

        protected override void InternalUpdateValue(float deltaChange)
        {
            SetValue(_startValue + _changeValue * deltaChange);
        }

        protected abstract float GetCurrentValue();
        protected abstract void SetValue(float valueChanged);
    }
}
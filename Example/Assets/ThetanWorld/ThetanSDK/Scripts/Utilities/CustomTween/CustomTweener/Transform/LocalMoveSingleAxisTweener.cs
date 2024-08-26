using UnityEngine;

namespace Wolffun.Tweening
{
    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2,
    }
    
    public sealed class LocalMoveSingleAxisTweener : FloatTweener
    {
        private Transform _transform;
        private Axis _axis;
        
        public void SetUp(Transform transform, Axis axis, float endPosition, float duration)
        {
            _transform = transform;
            _axis = axis;
            SetUp(endPosition, duration);
        }
        
        protected override float GetCurrentValue()
        {
            if (_transform)
            {
                switch (_axis)
                {
                    case Axis.X:
                        return _transform.localPosition.x;
                    case Axis.Y:
                        return _transform.localPosition.y;
                    case Axis.Z:
                        return _transform.localPosition.z;
                }
            }

            return 0;
        }

        protected override void SetValue(float valueChanged)
        {
            if (_transform)
            {
                var localPosition = _transform.localPosition;
                switch (_axis)
                {
                    case Axis.X:
                        localPosition.x = valueChanged;
                        break;
                    case Axis.Y:
                        localPosition.y = valueChanged;
                        break;
                    case Axis.Z:
                        localPosition.z = valueChanged;
                        break;
                }
                _transform.localPosition = localPosition;
            }
        }

        public override void ResetValue()
        {
            base.ResetValue();
            _transform = null;
            _axis = Axis.X;
        }
    }
}
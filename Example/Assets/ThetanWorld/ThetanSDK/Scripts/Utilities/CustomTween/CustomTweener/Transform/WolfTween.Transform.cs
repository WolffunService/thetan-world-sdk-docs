using UnityEngine;

namespace Wolffun.Tweening
{
    public static partial class WolfTween
    {
        public static Tweener DOScale(this Transform transform, Vector3 endValue, float duration)
        {
            var tweener = TweenManager.RentTween<LocalScaleTweener>();
            tweener.SetUp(transform, endValue, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }
        
        public static Tweener DOMove(this Transform transform, Vector3 endPosition, float duration)
        {
            var tweener = TweenManager.RentTween<WorldMovePositionTweener>();
            tweener.SetUp(transform, endPosition, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }

        public static Tweener DOSizeDelta(this RectTransform transform, Vector2 endValue, float duration)
        {
            var tweener = TweenManager.RentTween<SizeDeltaRectTransformTweener>();
            tweener.SetUp(transform, endValue, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }
        
        public static Tweener DOLocalMove(this Transform transform, Vector3 endPosition, float duration)
        {
            var tweener = TweenManager.RentTween<LocalMoveTweener>();
            tweener.SetUp(transform, endPosition, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }

        public static Tweener DOLocalMoveX(this Transform transform, float endPosition, float duration)
        {
            var tweener = TweenManager.RentTween<LocalMoveSingleAxisTweener>();
            tweener.SetUp(transform, Axis.X, endPosition, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }
        
        public static Tweener DOLocalMoveY(this Transform transform, float endPosition, float duration)
        {
            var tweener = TweenManager.RentTween<LocalMoveSingleAxisTweener>();
            tweener.SetUp(transform, Axis.Y, endPosition, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }
        
        public static Tweener DOLocalMoveZ(this Transform transform, float endPosition, float duration)
        {
            var tweener = TweenManager.RentTween<LocalMoveSingleAxisTweener>();
            tweener.SetUp(transform, Axis.Z, endPosition, duration);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }
    }
}
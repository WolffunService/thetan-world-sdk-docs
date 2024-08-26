using System;

namespace Wolffun.Tweening
{
    public static partial class DOVirtual
    {
        public static TweenerCore DelayedCall(float delay, Action callback)
        {
            var tweener = TweenManager.RentTween<DelayCallTweener>();
            tweener.SetUp(delay, callback, true);
            TweenManager.RegisterTween(tweener);
            return tweener;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.Tweening
{
    public sealed class TweenSequence : TweenerCore
    {
        private struct TweenTimeLine
        {
            public float startPosition; // position in timeline
            public float endPosition; // position in timeline
            public TweenerCore tweener;
        }
        
        protected Action _onCompleteCallback;
        protected float _duration;
        protected bool _isComplete;
        protected bool _isIndependentTimeScale;
        private float _currentTime;
        private bool _isStarted;
        
        private List<TweenTimeLine> _listTweenerInTimeline;
        private float _lastInsertPosition; // position in timeline

        public override bool IsStarted => _isStarted;
        public override bool IsComplete => _isComplete;
        public override float CurrentTime => _currentTime;
        public override float Duration => _duration;
        public override bool IsIndependentTimeScale => _isIndependentTimeScale;
        
        public float LastInsertPosition => _lastInsertPosition;
        
        public TweenSequence()
        {
            _listTweenerInTimeline = new List<TweenTimeLine>();
        }

        public override void ResetValue()
        {
            _onCompleteCallback = null;
            _duration = 0;
            _isComplete = false;
            _isIndependentTimeScale = true;
            _currentTime = 0;
            _listTweenerInTimeline.Clear();
            _isStarted = false;
        }

        public TweenSequence SetUpdate(bool isIndependentTimescale)
        {
            _isIndependentTimeScale = isIndependentTimescale;

            return this;
        }

        public void OnComplete(Action callback)
        {
            _onCompleteCallback = callback;
        }

        public void InsertAt(Tweener tweener, float timelinePosition)
        {
            TweenManager.AddActiveTweenToSequence(tweener);

            var startPosition = timelinePosition + tweener.Delay;
            
            TweenTimeLine tweenTimeLine = new TweenTimeLine();
            tweenTimeLine.startPosition = startPosition;
            tweenTimeLine.endPosition = startPosition + tweener.Duration;
            tweenTimeLine.tweener = tweener;
            tweener.SetDelay(0);
            
            _listTweenerInTimeline.Add(tweenTimeLine);

            if (tweenTimeLine.endPosition > _duration)
                _duration = tweenTimeLine.endPosition;
            
            _lastInsertPosition = startPosition;
        }

        public void InsertAt(TweenerCore tweenerCore, float timelinePosition)
        {
            TweenManager.AddActiveTweenToSequence(tweenerCore);

            var startPosition = timelinePosition;
            
            TweenTimeLine tweenTimeLine = new TweenTimeLine();
            tweenTimeLine.startPosition = startPosition;
            tweenTimeLine.endPosition = startPosition + tweenerCore.Duration;
            tweenTimeLine.tweener = tweenerCore;
            
            _listTweenerInTimeline.Add(tweenTimeLine);

            if (tweenTimeLine.endPosition > _duration)
                _duration = tweenTimeLine.endPosition;
            
            _lastInsertPosition = startPosition;
        }
        
        
        public override void UpdateWithDeltaTime(float dt)
        {
            if (!_isStarted)
                _isStarted = true;
            
            _currentTime += dt;
            _currentTime = Mathf.Min(_currentTime, _duration);

            UpdateWithCurrentTime(_currentTime, dt);
        }

        protected void UpdateWithCurrentTime(float currentTime, float deltaTime)
        {
            int countTweenComplete = 0;
            foreach (var tweenInTimeline in _listTweenerInTimeline)
            {
                if (currentTime >= tweenInTimeline.startPosition && !tweenInTimeline.tweener.IsComplete)
                {
                    tweenInTimeline.tweener.UpdateWithDeltaTime(deltaTime);
                }

                if (tweenInTimeline.tweener.IsComplete)
                    countTweenComplete++;
            }

            if (countTweenComplete == _listTweenerInTimeline.Count)
            {
                foreach (var tweenInTimeline in _listTweenerInTimeline)
                {
                    TweenManager.ReturnTween(tweenInTimeline.tweener.GetType(), tweenInTimeline.tweener);
                }

                _isComplete = true;
                _onCompleteCallback?.Invoke();
                
                _listTweenerInTimeline.Clear();
            }

        }
    }
}
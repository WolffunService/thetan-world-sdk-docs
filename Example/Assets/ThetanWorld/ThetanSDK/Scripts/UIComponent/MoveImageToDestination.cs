using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Tweening;
using Ease = Wolffun.Tweening.Ease;

namespace ThetanSDK.UI
{
    public class MoveImageToDestination : MonoBehaviour
    {
        [SerializeField] private Image _imgItem;
        [SerializeField] private RectTransform _startPosition;
        [SerializeField] private RectTransform _endPosition;

        [SerializeField] private float _fadeInDuration;
        [SerializeField] private Ease _fadeInEase;
        [SerializeField] private float _moveToTargetDuration;
        [SerializeField] private Ease _moveToTargetEase;
        [SerializeField] private float _fadeOutDelay;
        [SerializeField] private float _fadeOutDuration;
        [SerializeField] private Ease _fadeOutEase;

        private TweenSequence _curSequence;

        private void Awake()
        {
            _imgItem.SetAlphaImg(0);
        }

        [ContextMenu("PlayAnim")]
        public void PlayAnim()
        {
            if(_curSequence != null && !_curSequence.IsComplete)
                _curSequence.Complete();

            _imgItem.transform.position = _startPosition.position;
            _imgItem.SetAlphaImg(0);

            _curSequence = WolfTween.GetSequence();

            _curSequence.Append(_imgItem.DOFade(1, _fadeInDuration)
                .SetEase(_fadeInEase)
                .SetUpdate(true));
            _curSequence.Append(_imgItem.transform.DOMove(_endPosition.position, _moveToTargetDuration)
                .SetEase(_moveToTargetEase)
                .SetUpdate(true));
            _curSequence.Join(_imgItem.DOFade(0, _fadeOutDuration)
                .SetDelay(_fadeOutDelay)
                .SetEase(_fadeOutEase)
                .SetUpdate(true));
            _curSequence.SetUpdate(true);
        }
    }
}


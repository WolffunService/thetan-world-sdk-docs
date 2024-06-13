using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ShowToastBehavior : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _txtMsg;
        [SerializeField] private Button _btnCloseToast;

        [SerializeField] private RectTransform _startPosition;
        [SerializeField] private RectTransform _endPosision;
        
        [Tooltip("Total second text move from Start Position to End Position")]
        [SerializeField] private float _moveTextDuration;
        
        [Tooltip("Total second text stay at End Position before fade out")]
        [SerializeField] private float _idleTextDuration;

        [Tooltip("Total second text fade out")]
        [SerializeField] private float _fadeOutDuration;

        private Sequence _curTweenAnimation;
        
        private void Awake()
        {
            _txtMsg.text = string.Empty;
            _txtMsg.enabled = false;

            _btnCloseToast.image.raycastTarget = false;
            
            _btnCloseToast.onClick.AddListener(() =>
            {
                if(_curTweenAnimation != null)
                    _curTweenAnimation.Complete();
            });
        }

        public void ShowText(string msg)
        {
            if(_curTweenAnimation != null)
                _curTweenAnimation.Kill();

            _btnCloseToast.image.raycastTarget = true;
            
            _txtMsg.text = msg;
            _txtMsg.enabled = true;

            var color = _txtMsg.color;
            color.a = 1;
            _txtMsg.color = color;

            var rt = _txtMsg.transform as RectTransform;

            rt.position = _startPosition.position;

            _curTweenAnimation = DOTween.Sequence();
            
            _curTweenAnimation.Append(rt.DOMove(_endPosision.position, _moveTextDuration)
                .SetUpdate(true));
            _curTweenAnimation.Append(_txtMsg.DOFade(0, _fadeOutDuration)
                .SetDelay(_idleTextDuration)
                .SetUpdate(true));

            _curTweenAnimation.SetUpdate(true);
            _curTweenAnimation.OnComplete(() =>
            {
                _txtMsg.text = string.Empty;
                _txtMsg.enabled = false;
                _curTweenAnimation = null;
                
                _btnCloseToast.image.raycastTarget = false;
            });
        }
    }
}


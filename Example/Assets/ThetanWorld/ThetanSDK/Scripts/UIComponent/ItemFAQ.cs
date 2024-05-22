using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ItemFAQ : MonoBehaviour
    {
        [SerializeField] private RectTransform _rtContent;
        [SerializeField] private TextMeshProUGUI _txtTitle;

        [SerializeField] private RectTransform _rtDesc;

        [SerializeField] private TextMeshProUGUI _txtDesc;

        [SerializeField] private float _contentBaseSize;

        [SerializeField] private float _descBaseSize;

        [SerializeField] private Button _btnExpand;

        [SerializeField] private float _animDuration;

        [SerializeField] private Image _imgIconExpand;

        private bool _isCollapse;

        void Awake()
        {
            _btnExpand.onClick.AddListener(DoExpandOrCollapse);
            _isCollapse = true;
            _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _contentBaseSize);
            _rtDesc.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        public void Initialize(string title, string description)
        {
            _txtTitle.text = title;
            _txtDesc.text = description;

            if (_isCollapse)
                return;
            
            _isCollapse = true;
            _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _contentBaseSize);
            _rtDesc.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        public void CollapseDescription()
        {
            if (_isCollapse)
                return;
            
            _isCollapse = true;
            _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _contentBaseSize);
            _rtDesc.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        private void DoExpandOrCollapse()
        {
            if (_isCollapse)
            {
                DoAnimExpand();
            }
            else
            {
                DoAnimCollapse();
            }
        }

        private void DoAnimCollapse()
        {
            _btnExpand.interactable = false;

            _rtContent.DOSizeDelta(new Vector2(_rtContent.sizeDelta.x, _contentBaseSize), _animDuration)
                .OnComplete(() =>
                {
                    _isCollapse = true;
                    _btnExpand.interactable = true;
                    _imgIconExpand.transform.localRotation = Quaternion.Euler(0, 0, 0);
                });
            _rtDesc.DOSizeDelta(new Vector2(_rtDesc.sizeDelta.x, 0), _animDuration);
        }

        private void DoAnimExpand()
        {
            _btnExpand.interactable = false;

            float expandSize = _txtDesc.preferredHeight;

            _rtContent.DOSizeDelta(new Vector2(_rtContent.sizeDelta.x, _contentBaseSize + _descBaseSize + expandSize),
                    _animDuration)
                .OnComplete(() =>
                {
                    _isCollapse = false;
                    _btnExpand.interactable = true;
                    _imgIconExpand.transform.localRotation = Quaternion.Euler(0, 0, 180);
                });
            _rtDesc.DOSizeDelta(new Vector2(_rtDesc.sizeDelta.x, _descBaseSize + expandSize), _animDuration);
        }

        [ContextMenu("Auto Calculate Size")]
        private void AutoCalculateSize()
        {
            _contentBaseSize = _rtContent.sizeDelta.y;
            _descBaseSize = _rtDesc.sizeDelta.y - _txtDesc.preferredHeight;
        }
    }
}
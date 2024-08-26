using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Wolffun.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    [Serializable]
    public enum TooltipAlignment
    {
        TopLeft = 0,
        TopMiddle = 1,
        TopRight = 2,
        LeftBottom = 10,
        LeftMiddle = 11,
        LeftTop = 12,
        RightBottom = 20,
        RightMiddle = 21,
        RightTop = 22,
        BottomLeft = 30,
        BottomMiddle = 31,
        BottomRight = 32
    }
    
    public class TooltipText : Popup
    {
        [Serializable]
        private struct ArrowAlignmentConfig
        {
            public TooltipAlignment alignment;
            public RectTransform arrow;
        }
        
        [SerializeField] private Button _btnAutoHideTooltip;
        [SerializeField] private RectTransform _content;
        [SerializeField] private TextMeshProUGUI _txtInfo;
        [SerializeField] private List<ArrowAlignmentConfig> _listArrowsAllAligment;
        [SerializeField] private float baseContentHeight;
        [SerializeField] private float durationIn;
        [SerializeField] private float durationOut;

        private bool isInTransition = false;
        
        private void Awake()
        {
            _btnAutoHideTooltip.onClick.AddListener(HideTooltip);
            _btnAutoHideTooltip.interactable = false;
            _btnAutoHideTooltip.image.raycastTarget = false;
            _content.localScale = Vector3.zero;
        }

        public override void OnBeforePushPopup()
        {
            base.OnBeforePushPopup();
            isInTransition = true;
        }

        public override void OnBeforePopPopup()
        {
            base.OnBeforePopPopup();
            isInTransition = true;
        }

        public override void OnAfterPushPopup()
        {
            base.OnAfterPushPopup();
            isInTransition = false;
        }

        public override void OnAfterPopPopup()
        {
            base.OnAfterPopPopup();
            isInTransition = false;
        }

        public async void ShowTooltip(string msg, RectTransform positionTransform, TooltipAlignment alignment)
        {
            try
            {
                if (isInTransition)
                    await UniTask.WaitUntil(() => !isInTransition,
                        cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException canceledException)
            {
                return;
            }
            
            _txtInfo.text = msg;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _txtInfo.preferredHeight + baseContentHeight);

            alignment = CorrectAlignmentSnapToViewport(positionTransform, alignment);
            
            RectTransform selectedArrow = null;
            foreach (var pair in _listArrowsAllAligment)
            {
                if (pair.alignment != alignment)
                {
                    pair.arrow.gameObject.SetActive(false);
                }
                else
                {
                    selectedArrow = pair.arrow;
                    pair.arrow.gameObject.SetActive(true);
                }
            }

            var tempPivot = GetNormalizedPos(_content, selectedArrow);
            switch (alignment)
            {
                case TooltipAlignment.TopLeft:
                case TooltipAlignment.TopMiddle:
                case TooltipAlignment.TopRight:
                    tempPivot.y = 1;
                    break;
                case TooltipAlignment.BottomLeft:
                case TooltipAlignment.BottomMiddle:
                case TooltipAlignment.BottomRight:
                    tempPivot.y = 0;
                    break;
                case TooltipAlignment.LeftBottom:
                case TooltipAlignment.LeftMiddle:
                case TooltipAlignment.LeftTop:
                    tempPivot.x = 0;
                    break;
                case TooltipAlignment.RightBottom:
                case TooltipAlignment.RightMiddle:
                case TooltipAlignment.RightTop:
                    tempPivot.x = 1;
                    break;
            }
            _content.pivot = tempPivot;
            _content.position = positionTransform.position;
            
            _content.localScale = Vector3.zero;
            _content.DOScale(Vector3.one, durationIn)
                .SetUpdate(true)
                .OnComplete(() =>
            {
                _btnAutoHideTooltip.interactable = true;
                _btnAutoHideTooltip.image.raycastTarget = true;
            });
        }
        
        public void HideTooltip()
        {
            _btnAutoHideTooltip.interactable = false;
            _btnAutoHideTooltip.image.raycastTarget = false;
            _content.DOScale(Vector3.zero, durationOut)
                .SetUpdate(true)
                .OnComplete(() =>
            {
                if (_popupContainer.CurrentPopup == this)
                {
                    _popupContainer.Pop();
                }
            });
        }

        [ContextMenu("AutoGetBaseContentHeight")]
        private void AutoGetBaseContentHeight()
        {
#if UNITY_EDITOR
            baseContentHeight = _content.rect.height - _txtInfo.preferredHeight;
            EditorUtility.SetDirty(this);
#endif
        }
        
        private Vector2 GetNormalizedPos(RectTransform parentRect, RectTransform childRect)
        {
            var parentSize = new Vector2(parentRect.rect.width, parentRect.rect.height);
            var recalPos = (Vector2)childRect.localPosition + parentSize / 2.0f;

            return new Vector2(
                    Mathf.Clamp(recalPos.x / parentSize.x, 0, 1),
                    Mathf.Clamp(recalPos.y / parentSize.y, 0, 1))
                ;
        }

        private enum MainAlignment
        {
            Top = 0,
            Left = 1,
            Right = 2,
            Bottom = 3,
        }
        
        private TooltipAlignment CorrectAlignmentSnapToViewport(RectTransform targetPosition, TooltipAlignment alignment)
        {
            var contentSize = _content.rect.size;
            var viewPortSize = (_content.parent as RectTransform).rect.size;

            var localPosition = _content.parent.InverseTransformPoint(targetPosition.position)
                                + new Vector3(viewPortSize.x / 2, viewPortSize.y / 2, 0);

            var mainAlignment = (MainAlignment)((int)alignment / 10);
            int secondAlignment = (int)alignment % 10;
            float secondAlignmentPosition = 0;
            float secondAlignmentSize = 0;
            float secondAlignmentMax = 0;
            
            // Correct left alignment
            if (mainAlignment == MainAlignment.Left)
            {
                if (localPosition.x + contentSize.x > viewPortSize.x)
                    mainAlignment = FlipMainAlignment(mainAlignment);

                secondAlignmentPosition = localPosition.y;
                secondAlignmentSize = contentSize.y;
                secondAlignmentMax = viewPortSize.y;
            }
            else if (mainAlignment == MainAlignment.Right)
            {
                if (localPosition.x - contentSize.x < 0)
                    mainAlignment = FlipMainAlignment(mainAlignment);
                
                secondAlignmentPosition = localPosition.y;
                secondAlignmentSize = contentSize.y;
                secondAlignmentMax = viewPortSize.y;
            }
            else if (mainAlignment == MainAlignment.Bottom)
            {
                if (localPosition.y + contentSize.y > viewPortSize.y)
                    mainAlignment = FlipMainAlignment(mainAlignment);
                
                secondAlignmentPosition = localPosition.x;
                secondAlignmentSize = contentSize.x;
                secondAlignmentMax = viewPortSize.x;
            }
            else if (mainAlignment == MainAlignment.Top)
            {
                if (localPosition.y - contentSize.y < 0)
                    mainAlignment = FlipMainAlignment(mainAlignment);
                
                secondAlignmentPosition = localPosition.x;
                secondAlignmentSize = contentSize.x;
                secondAlignmentMax = viewPortSize.x;
            }

            if (secondAlignment == 0 &&
                secondAlignmentPosition + secondAlignmentSize > secondAlignmentMax)
            {
                secondAlignment = 2;
            }
            else if (secondAlignment == 2 &&
                     secondAlignmentPosition - secondAlignmentSize < 0)
            {
                secondAlignment = 0;
            }
            else if (secondAlignment == 1)
            {
                if (secondAlignmentPosition + secondAlignmentSize / 2 > secondAlignmentMax)
                {
                    secondAlignment = 2;
                }
                else if (secondAlignmentPosition - secondAlignmentSize / 2 < 0)
                {
                    secondAlignment = 0;
                }
            }

            return (TooltipAlignment)((int)mainAlignment * 10 + secondAlignment);
        }

        private MainAlignment FlipMainAlignment(MainAlignment alignment)
        {
            return (MainAlignment)(3 - (int)alignment);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMsg : Popup
{
    [SerializeField] private Image _imgGreyBg;
    [SerializeField] private CanvasGroup _canvasGroupPopup;
    [SerializeField] private RectTransform _popupContent;
    [SerializeField] private TextMeshProUGUI _txtTitle;
    [SerializeField] private TextMeshProUGUI _txtMsg;
    [SerializeField] private float _defaultPopupSize;
    [SerializeField] private float _maxPopupSize;
    
    [Header("1 Button")]
    [SerializeField] private GameObject _content1Button;
    [SerializeField] private TextMeshProUGUI _txtButton;
    [SerializeField] private Button _btnConfirm;
    
    [Header("2 Buttons")]
    [SerializeField] private GameObject _content2Button;
    [SerializeField] private TextMeshProUGUI _txtButtonLeft;
    [SerializeField] private Button _btnConfirmLeft;
    [SerializeField] private TextMeshProUGUI _txtButtonRight;
    [SerializeField] private Button _btnConfirmRight;

    [Header("Animation config")]
    [SerializeField] private float showPopupDuration;
    [SerializeField] private Ease showPopupEase;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private Ease fadeOutEase;

    private Action _onConfirmCallback;
    private Action _onLeftConfirmCallback;
    private Action _onRightConfirmCallback;
    
    private void Awake()
    {
        _imgGreyBg.gameObject.SetActive(false);

        _canvasGroupPopup.alpha = 0;
        _canvasGroupPopup.blocksRaycasts = false;
        _canvasGroupPopup.interactable = false;
        
        _popupContent.localScale = Vector3.zero;
        
        _btnConfirm.onClick.AddListener(OnClickConfirm);
        _btnConfirmLeft.onClick.AddListener(OnClickLeftConfirm);
        _btnConfirmRight.onClick.AddListener(OnClickRightConfirm);
    }
    
    private void OnClickConfirm()
    {
        _onConfirmCallback?.Invoke();
        ClosePopUp();
    }

    private void OnClickLeftConfirm()
    {
        _onLeftConfirmCallback?.Invoke();
        ClosePopUp();
    }

    private void OnClickRightConfirm()
    {
        _onRightConfirmCallback?.Invoke();
        ClosePopUp();
    }
    
    public void ShowPopUp(string title, string msg, string buttonText, Action onConfirmCallback)
    {
        _content1Button.SetActive(true);
        _content2Button.SetActive(false);
        
        _onConfirmCallback = onConfirmCallback;

        _txtTitle.text = title;
        _txtMsg.text = msg;
        _txtButton.text = buttonText;
        
        _imgGreyBg.gameObject.SetActive(true);

        _canvasGroupPopup.alpha = 1;
        _canvasGroupPopup.blocksRaycasts = true;
        _canvasGroupPopup.interactable = true;

        SetSizePopupBasedOnMsg();

        _popupContent.DOScale(Vector3.one, showPopupDuration).SetEase(showPopupEase);
    }

    public void ShowPopUp(string title, string msg, string buttonTextLeft, string buttonTextRight,
        Action onLeftConfirmCallback, Action onRightConfirmCallback)
    {
        _content1Button.SetActive(false);
        _content2Button.SetActive(true);

        _onLeftConfirmCallback = onLeftConfirmCallback;
        _onRightConfirmCallback = onRightConfirmCallback;
        
        _txtTitle.text = title;
        _txtMsg.text = msg;

        _txtButtonLeft.text = buttonTextLeft;
        _txtButtonRight.text = buttonTextRight;
        
        _imgGreyBg.gameObject.SetActive(true);

        _canvasGroupPopup.alpha = 1;
        _canvasGroupPopup.blocksRaycasts = true;
        _canvasGroupPopup.interactable = true;

        SetSizePopupBasedOnMsg();

        _popupContent.DOScale(Vector3.one, showPopupDuration).SetEase(showPopupEase);
    }

    private void SetSizePopupBasedOnMsg()
    {
        var rt = _popupContent.transform as RectTransform;
        var prefferedPopupSize = _defaultPopupSize + _txtMsg.preferredHeight;
        prefferedPopupSize = Mathf.Clamp(prefferedPopupSize, 0, _maxPopupSize);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, prefferedPopupSize);
    }

    public void ClosePopUp()
    {
        _onConfirmCallback = null;
        _canvasGroupPopup.DOFade(0, fadeOutDuration)
            .SetEase(fadeOutEase)
            .OnComplete(() =>
            {
                _imgGreyBg.gameObject.SetActive(false);
                
                _canvasGroupPopup.alpha = 0;
                _canvasGroupPopup.blocksRaycasts = false;
                _canvasGroupPopup.interactable = false;

                _popupContent.localScale = Vector3.zero;
                
                if(_popupContainer.CurrentPopup == this)
                    _popupContainer.Pop();
            });
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Calculate Default Popup Size")]
    public void AutoCalculateDefaultPopupSize()
    {
        Debug.Log(_txtMsg.preferredHeight);
        var rt = _popupContent.transform as RectTransform;
        _defaultPopupSize = rt.sizeDelta.y - _txtMsg.preferredHeight;
        _maxPopupSize = rt.sizeDelta.y;
    }
#endif
}

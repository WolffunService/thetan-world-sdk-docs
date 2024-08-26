using ThetanSDK.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wolffun.Tweening;

public class PopupTutorialStep : Popup
{
    [SerializeField] private TextMeshProUGUI _txt;
    [SerializeField] private Button _btn;

    [SerializeField] private RectTransform _main;
    [SerializeField] private RectTransform _topMiddle;
    [SerializeField] private RectTransform _leftMiddle;

    private TooltipAlignment tooltipAlignment;
    private Vector3 position;

    public void Initialize(string txt, UnityAction action, RectTransform rectTransform, TooltipAlignment alignment)
    {
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(() => _popupContainer.Pop());
        _btn.onClick.AddListener(action);

        _txt.text = txt;

        position = rectTransform.position;
        tooltipAlignment = alignment;
    }

    public override void OnBeforePushPopup()
    {
        base.OnBeforePushPopup();
        _main.localScale = Vector3.zero;
    }

    public override void OnAfterPushPopup()
    {
        base.OnAfterPushPopup();

        _leftMiddle.gameObject.SetActive(tooltipAlignment == TooltipAlignment.LeftMiddle);
        _topMiddle.gameObject.SetActive(tooltipAlignment == TooltipAlignment.TopMiddle);

        _main.pivot = tooltipAlignment == TooltipAlignment.LeftMiddle ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);

        _main.position = position;

        _main.DOScale(Vector3.one, 0.2f);
    }
}

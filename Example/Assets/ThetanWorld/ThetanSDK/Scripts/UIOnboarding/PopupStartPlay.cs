using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupStartPlay : Popup
{
    [SerializeField] private Button _closeBtn;
    [SerializeField] private Button _playBtn;
    private Action _onCloseAction;

    private void Awake()
    {
        _closeBtn.onClick.AddListener(ClaimFreeNFT);
        _playBtn.onClick.AddListener(Play);
    }

    private void OnDestroy()
    {
        _closeBtn.onClick.RemoveAllListeners();
        _playBtn.onClick.RemoveAllListeners();
    }

    private void Play()
    {
        _popupContainer.Pop();
        _onCloseAction?.Invoke();
    }

    private void ClaimFreeNFT()
    {
        _popupContainer.Pop();
        _onCloseAction?.Invoke();
    }

    public void Initialize(Action action) => _onCloseAction = action;
}

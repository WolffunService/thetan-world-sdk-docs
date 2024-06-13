using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThetanSDK.UI;
using UnityEngine;
using UnityEngine.UI;

public class LoginMainUI_Mini : LoginMainUI
{
    [Header("Section UI Mini")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Button _btnCloseScreenBg;
    
    [Header("Config Anim UI Mini")]
    [SerializeField] private float _showScreenAnimDuration;
    [SerializeField] private Ease _showScreenAnimEase;

    private void Awake()
    {
        //_btnCloseScreenBg.onClick.AddListener(OnClickBgCloseScreen);
        
        _content.transform.localScale = Vector3.zero;
    }

    protected override void OnAfterPushScreen()
    {
        base.OnAfterPushScreen();

        _content.transform.DOScale(Vector3.one, _showScreenAnimDuration)
            .SetEase(_showScreenAnimEase)
            .SetUpdate(true);
    }

    private void OnClickBgCloseScreen()
    {
        _screenContainer.NotifyOnClickCloseScreen();
    }
}

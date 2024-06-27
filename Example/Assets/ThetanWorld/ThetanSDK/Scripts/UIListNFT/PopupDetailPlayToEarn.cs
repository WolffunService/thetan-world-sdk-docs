using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

public class PopupDetailPlayToEarn : Popup
{
    [SerializeField] private Button _btnClose1;
    [SerializeField] private Button _btnClose2;
    [SerializeField] private TextMeshProUGUI txtEquipment;
    [SerializeField] private TextMeshProUGUI txtEquipmentEffect;
    [SerializeField] private TextMeshProUGUI txtMaxEquipmentEffect;
    [SerializeField] private TextMeshProUGUI txtGrindStage;
    [SerializeField] private TextMeshProUGUI txtGrindSpeed;

    private void Awake()
    {
        _btnClose1.onClick.AddListener(OnClickClose);
        _btnClose2.onClick.AddListener(OnClickClose);
    }

    private void OnClickClose()
    {
        _popupContainer.Pop();
    }

    public void SetData(DetailHeroGrindInfo data)
    {
        txtEquipment.SetText("{0}/{1}", 0, 0);
        txtEquipmentEffect.SetText("{0}%", data.grindRewardBonus);
        txtMaxEquipmentEffect.SetText("(Max={0}%)", data.maxGrindRewarBonus);
        txtGrindStage.SetText("{0}/{1}", data.stage, data.maxStage);
        txtGrindSpeed.SetText("{0}/h", data.currentGrindSpeed);
    }
}

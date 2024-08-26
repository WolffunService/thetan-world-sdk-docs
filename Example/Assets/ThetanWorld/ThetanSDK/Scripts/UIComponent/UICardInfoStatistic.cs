using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICardInfoStatistic : MonoBehaviour
{
    [SerializeField] private Button _btnShowTooltipInfo;
    [SerializeField] private GameObject _contentInfo;
    [SerializeField] private TextMeshProUGUI _txtValue;
    [SerializeField] private TextMeshProUGUI _txtBonus;
    [SerializeField] private string formatStrBonus = "+{0:N2}";

    [SerializeField] private GameObject _contentEmpty;


    private UIHelperContainer _uiHelperContainer;
    private string _txtTooltipInfo;

    private void Awake()
    {
        _btnShowTooltipInfo.onClick.AddListener(OnClickShowTooltipInfo);
    }

    public void Initialize(UIHelperContainer uiHelperContainer, string txtTooltipInfo)
    {
        _uiHelperContainer = uiHelperContainer;
        _txtTooltipInfo = txtTooltipInfo;
    }
    
    public void SetData(float value, float unclaimedValue)
    {
        if (value <= 0 && unclaimedValue <= 0)
        {
            _contentEmpty.SetActive(true);
            _contentInfo.SetActive(false);
        }
        else
        {
            _contentEmpty.SetActive(false);
            _contentInfo.SetActive(true);

            _txtValue.text = value.ToString("N2");
            _txtBonus.text = string.Format(formatStrBonus, unclaimedValue);
        }
    }

    public void SetData(float value, float unclaimedValue, string formatBonus)
    {
        if (value <= 0 && unclaimedValue <= 0)
        {
            _contentEmpty.SetActive(true);
            _contentInfo.SetActive(false);
        }
        else
        {
            _contentEmpty.SetActive(false);
            _contentInfo.SetActive(true);

            _txtValue.text = value.ToString("N2");
            _txtBonus.text = string.Format(formatBonus, unclaimedValue);
        }
    }
    
    private void OnClickShowTooltipInfo()
    {
        if (_uiHelperContainer != null)
            _uiHelperContainer.ShowTextTooltip(_txtTooltipInfo, _btnShowTooltipInfo.transform as RectTransform,
                TooltipAlignment.TopMiddle);
    }
}

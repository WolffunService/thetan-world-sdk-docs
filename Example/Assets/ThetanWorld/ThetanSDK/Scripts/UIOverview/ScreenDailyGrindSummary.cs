using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using UnityEngine;
using UnityEngine.UI;

public class ScreenDailyGrindSummary : ScreenContentMainUI
{
    [SerializeField] private Toggle _toggleByGame;
    [SerializeField] private ListDailyGrindNFTSummaryAdapter _listSummaryNFT;
    [SerializeField] private ListDailyGrindGameSummaryAdapter _listSummaryGame;

    public override void OnBeforePushScreen()
    {
        base.OnBeforePushScreen();

        _toggleByGame.isOn = false;
        HandleToggleByGameValueChange(false);
        _toggleByGame.onValueChanged.AddListener(HandleToggleByGameValueChange);
    }
    
    public override void OnAfterPopScreen()
    {
        base.OnAfterPopScreen();
        
        _toggleByGame.onValueChanged.RemoveAllListeners();
        
        _listSummaryGame.ClearData();
        _listSummaryNFT.ClearData();
    }
    
    private void HandleToggleByGameValueChange(bool isByGame)
    {
        if (isByGame)
        {
            _listSummaryNFT.gameObject.SetActive(false);
            _listSummaryGame.gameObject.SetActive(true);
            _listSummaryGame.ShowUI();
        }
        else
        {
            _listSummaryGame.gameObject.SetActive(false);
            _listSummaryNFT.gameObject.SetActive(true);
            _listSummaryNFT.ShowUI();
        }
    }

}

using System;
using ThetanSDK;
using ThetanSDK.UI;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupAccountInfo : Popup
{
    [Header("UI")]
    [SerializeField] private UserProfileAvatar _userProfileAvatar;
    [SerializeField] private TextMeshProUGUI _txtUserName;
    [SerializeField] private TextMeshProUGUI _txtUserId;
    [SerializeField] private TextMeshProUGUI _txtEmail;
    

    [Header("Action")]
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnCopyID;
    [SerializeField] private Button _btnConnectThetanID;

    [Header("LogOut")]
    [SerializeField] private LogoutUIFunction _logoutUIFunction;
    [SerializeField] private Transform _transformBtnLogOut;
    [SerializeField] private Vector3 _btnLogOut1ButtonPosition;
    [SerializeField] private Vector3 _btnLogOut2ButtonPosition;
    
    private UIHelperContainer _uiHelperContainer;

    private Action _onUserLogOutCallback;
    
    private void Awake()
    {
        _btnClose.onClick.AddListener(Close);
        _btnCopyID.onClick.AddListener(CopyID);
        _btnConnectThetanID.onClick.AddListener(ConnectThetanID);
    }

    
    public override void OnBeforePushPopup()
    {
        base.OnBeforePushPopup();
        
        var profileService = ThetanSDKManager.Instance.ProfileService;

        _userProfileAvatar.SetUI(profileService.AvatarId, profileService.AvatarFrameId);
        _txtUserName.SetText(profileService.Username);
        _txtUserId.SetText(profileService.UserId);

        if (string.IsNullOrEmpty(profileService.Email))
        {
            _txtEmail.SetText(string.Empty);
            _btnConnectThetanID.gameObject.SetActive(true);
            _transformBtnLogOut.localPosition = _btnLogOut2ButtonPosition;
        }
        else
        {
            _txtEmail.SetText(profileService.Email);
            _btnConnectThetanID.gameObject.SetActive(false);
            _transformBtnLogOut.localPosition = _btnLogOut1ButtonPosition;
        }
    }

    public override void OnAfterPopPopup()
    {
        base.OnAfterPopPopup();

        _onUserLogOutCallback = null;
    }

    public void Initialize(UIHelperContainer uiHelperContainer, Action onUserLogOutCallback)
    {
        _onUserLogOutCallback = onUserLogOutCallback;
        _uiHelperContainer = uiHelperContainer;
        _logoutUIFunction.Initialize(_uiHelperContainer, OnConfirmUserLogOut);
    }

    private void OnConfirmUserLogOut()
    {
        ThetanSDKManager.Instance.LogOut();
        _popupContainer.Pop();
        _onUserLogOutCallback?.Invoke();
    }


    private void OnDestroy()
    {
        _btnClose.onClick.RemoveAllListeners();
        _btnCopyID.onClick.RemoveAllListeners();
        _btnConnectThetanID.onClick.RemoveAllListeners();
    }

    private void Close() => _popupContainer.Pop();

    private void CopyID()
    {
        ThetanSDKManager.Instance.ProfileService.UserId.CopyToClipboard();
        if (_uiHelperContainer)
        {
            _uiHelperContainer.ShowToast("Copied to clipboard");
        }
    }

    [ContextMenu("AutoGetButtonLogOutConfig")]
    public void AutoGetButtonConfig()
    {
#if UNITY_EDITOR
        var localPosition = _transformBtnLogOut.transform.localPosition;
        _btnLogOut2ButtonPosition = localPosition;
        _btnLogOut1ButtonPosition = new Vector3(0,  localPosition.y, 0);
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    private void ConnectThetanID() 
    {
        ThetanSDKManager.Instance.ShowLinkAccount((AuthenResultData) =>
        {
            Close();
        }, null);
    }
}

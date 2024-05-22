using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKServices.Equipment
{
    internal class EquipmentService : BaseClassService
    {
        private const int MAXIMUM_STAR_FOR_EQUIPMENT = 3;
        private Dictionary<string, EquipmentItemData> _listEquipments = new Dictionary<string, EquipmentItemData>();

        private ThetanNetworkClientState _prevNetworkClientState;
        private NetworkClient _networkClient;

        public async UniTask InitService(NetworkClient networkClient)
        {
            _networkClient = networkClient;
            _prevNetworkClientState = networkClient.NetworkClientState;
            
            networkClient.SubcribeOnChangeNetworkClientState(OnChangeNetworkClientState);
            
            if (networkClient.NetworkClientState != ThetanNetworkClientState.LoggedIn)
                return;
            
            UniTaskCompletionSource completionSource = new UniTaskCompletionSource();

            FetchListItem(_ => completionSource.TrySetResult(), _ => completionSource.TrySetResult());

            await completionSource.Task;
        }
        
        private void OnDestroy()
        {
            if (_networkClient != null)
                _networkClient.UnSubcribeOnChangeNetworkClientState(OnChangeNetworkClientState);
        }
        
        private void OnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedIn)
            {
                FetchListItem(null, null);
            }
            else if ((newState == ThetanNetworkClientState.NotLoggedIn ||
                      newState == ThetanNetworkClientState.NotLoggedInNoNetwork) &&
                     (_prevNetworkClientState == ThetanNetworkClientState.LoggedIn || 
                      _prevNetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork))
            {
                ClearDataService();
            }
        }
        
        public override void ClearDataService()
        {
            if (_listEquipments == null)
                _listEquipments = new Dictionary<string, EquipmentItemData>();
            
            _listEquipments.Clear();
        }

        public bool TryGetEquipmentData(string equipmentId, out EquipmentItemData equipmentData)
        {
            if (string.IsNullOrEmpty(equipmentId))
            {
                equipmentData = new EquipmentItemData().SetDefault();
                return false;
            }
                
            if (_listEquipments == null)
                _listEquipments = new Dictionary<string, EquipmentItemData>();

            return _listEquipments.TryGetValue(equipmentId, out equipmentData);
        }

        public bool CheckEquipmentCanUpgrade(string equipmentId)
        {
            if (!TryGetEquipmentData(equipmentId, out var equipmentToUpgrade))
                return false;

            if (equipmentToUpgrade.props.stars >= MAXIMUM_STAR_FOR_EQUIPMENT)
                return false;

            foreach (var kp in _listEquipments)
            {
                if(equipmentToUpgrade.id == kp.Key)
                    continue;

                var equipmentUsedForUpgrade = kp.Value;

                if (equipmentUsedForUpgrade.props.status != EquipmentStatus.EquipmentStatusDefault)
                    continue;

                if (equipmentUsedForUpgrade.kind != equipmentToUpgrade.kind ||
                    equipmentUsedForUpgrade.type != equipmentToUpgrade.type)
                    continue;

                var maximumStarItemUsedForUpgrade = MAXIMUM_STAR_FOR_EQUIPMENT - equipmentToUpgrade.props.stars;
                
                if (equipmentUsedForUpgrade.props.stars > maximumStarItemUsedForUpgrade)
                    continue;

                return true;
            }

            return false;
        }

        public bool CheckHaveAnyEquipmentForSlot(int equipmentTypeId)
        {
            foreach (var kp in _listEquipments)
            {
                var equipmentToEquip = kp.Value;

                if (equipmentToEquip.props.status != EquipmentStatus.EquipmentStatusDefault)
                    continue;

                if (equipmentToEquip.kind != 5 ||
                    equipmentToEquip.type != equipmentTypeId)
                    continue;

                return true;
            }

            return false;
        }

        public void FetchListItem(Action<List<EquipmentItemData>> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (ThetanSDKManager.Instance.NetworkClientState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                onErrorCallback?.Invoke(new WolffunResponseError(-1, "No internet connection"));
                return;
            }
            
            if (ThetanSDKManager.Instance.NetworkClientState != ThetanNetworkClientState.LoggedIn)
            {
                onErrorCallback?.Invoke(new WolffunResponseError(-99, "User is not in logged in state"));
                return;
            }
            
            WolffunRequestCommon reqCommon = WolffunRequestCommon
                .Create(WolffunUnityHttp.Settings.ThetanWorldURL + "/inventory/user")
                .GetQuery(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("kind", "6")
                });
            
            WolffunUnityHttp.Instance.MakeAPI<List<EquipmentItemData>>(reqCommon, listEquipment =>
            {
                if (_listEquipments == null)
                    _listEquipments = new Dictionary<string, EquipmentItemData>();
                
                _listEquipments.Clear();

                foreach (var equipment in listEquipment)
                {
                    _listEquipments[equipment.id] = equipment;
                }
                
                onSuccessCallback?.Invoke(listEquipment);
            }, onErrorCallback, AuthType.TOKEN);
        }
    }
}
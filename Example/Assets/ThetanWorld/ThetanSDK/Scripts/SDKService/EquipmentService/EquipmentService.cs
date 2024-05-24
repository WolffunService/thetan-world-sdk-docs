using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKService;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKServices.Equipment
{
    internal class EquipmentService : BaseClassService
    {
        /// <summary>
        /// Maximum star for each equipment
        /// </summary>
        private const int MAXIMUM_STAR_FOR_EQUIPMENT = 3;
        
        /// <summary>
        /// list equipments of user that cached locally
        /// </summary>
        private Dictionary<string, EquipmentItemData> _listEquipments = new Dictionary<string, EquipmentItemData>();

        /// <summary>
        /// Previous network client state
        /// </summary>
        private ThetanNetworkClientState _prevNetworkClientState;

        private NetworkClient _networkClient;

        /// <summary>
        /// Call to init service. Must call before use any service's other functions
        /// </summary>
        /// <param name="networkClient"></param>
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
        
        /// <summary>
        /// Clear all cached
        /// </summary>
        public override void ClearDataService()
        {
            if (_listEquipments == null)
                _listEquipments = new Dictionary<string, EquipmentItemData>();
            
            _listEquipments.Clear();
        }

        /// <summary>
        /// Try get equipment data in local cached.
        /// Return true if local cache has data, other wise return false
        /// </summary>
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

        /// <summary>
        /// Check if equipment can be upgraded
        /// </summary>
        public bool CheckEquipmentCanUpgrade(string equipmentId)
        {
            if (!TryGetEquipmentData(equipmentId, out var equipmentToUpgrade))
                return false;

            /* Each equipment can only upgrade to maximum star.
             * If maximum is reached, equipment cannot be upgraded anymore */
            if (equipmentToUpgrade.props.stars >= MAXIMUM_STAR_FOR_EQUIPMENT)
                return false;

            /* Loop through list of equipment.
             * Find 1 equipment that has the same kind and type of equipment want to upgrade.
             * And check if that equipment has number of star less than total star equipment want to upgrade can upgrade*/
            foreach (var kp in _listEquipments)
            {
                /* Item used for upgrade cannot be the same as Item want to upgrade */
                if(equipmentToUpgrade.id == kp.Key)
                    continue;

                var equipmentUsedForUpgrade = kp.Value;

                /* If item is used or selling on market, cannot be used for upgrade */
                if (equipmentUsedForUpgrade.props.status != EquipmentStatus.EquipmentStatusDefault)
                    continue;

                if (equipmentUsedForUpgrade.kind != equipmentToUpgrade.kind ||
                    equipmentUsedForUpgrade.type != equipmentToUpgrade.type)
                    continue;

                /* Item used for upgrade cannot have more star than the star item to upgrade missing
                 * Example: Item need to upgrade is at 1 star, maximum star is 3,
                 * so it can only use item that has 2 or less star for item used for upgrade*/
                var maximumStarItemUsedForUpgrade = MAXIMUM_STAR_FOR_EQUIPMENT - equipmentToUpgrade.props.stars;
                if (equipmentUsedForUpgrade.props.stars > maximumStarItemUsedForUpgrade)
                    continue;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check user have any equipment that have equipmentTypeId that still available for equip
        /// </summary>
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

        /// <summary>
        /// Call server to fetch list user's equipment and cache data locally
        /// </summary>
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
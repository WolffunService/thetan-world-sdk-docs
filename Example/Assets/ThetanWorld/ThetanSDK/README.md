# INTRODUCTION
This SDK allow player to select NFT, and bring NFT into game to play and earn THG. The process user play and earn THG through playing an NFT is called **Grind**

Grind process with be splitted into phases that depend on your game to decide when to invoke which phase. Each phase is descript as following:
- **Select NFT**: User can select NFT through Thetan World UI which is integrated inside SDK. After select NFT success, user can start grind NFT.
- **Prepare Grind**: This phase is called by your game to lock selected NFT and start a grind session. User cannot unselect NFT or select other NFT. User also cannot call prepare for another NFT when there is one NFT is locked for grind.
- **Start Grind**: This phase is called by your game to start grind for NFT. Grinding will be count every 5 second tick.
- **Pause Grind**: This phase is optional, you can call this phase when your game mechanic support pause game when playing or something similar to that. You can resume grinding by calling Start Grind again.
- **End Grind**: This phase is called by your game to end grinding session and unlock selected NFT.
- **Unlock UI Thetan World**: This phase is called by your game to unlock Thetan World UI interaction after grinding session is completed.

# INSTALL GUIDE
**Step 1:** Add these packages into your project
- "com.zbase.collections.pooled": "https://github.com/Zitga-Tech/ZBase.Collections.Pooled.git?path=Packages/ZBase.Collections.Pooled",
- "com.zbase.foundation.pooling": "https://github.com/Zitga-Tech/ZBase.Foundation.Pooling.git?path=Packages/ZBase.Foundation.Pooling",
- "com.wolffun.download-from-storage": "https://github.com/WolffunGame/DownloadFromStorage.git#1.0.15",
- "com.unity.nuget.newtonsoft-json": "3.2.1",
- "com.wolffun.log": "https://github.com/WolffunGame/Wolffun-Log.git#1.0.1",
- "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
- "com.cysharp.zstring": "https://github.com/Cysharp/ZString.git?path=src/ZString.Unity/Assets/Scripts/ZString",

And also add this Scope Registry into your project
```
"name": "Unity NuGet",
"url": "https://unitynuget-registry.azurewebsites.net",
"scopes": [
    "org.nuget"
]
```

**Step 2:**: Import ThetanWorldSDK.unitypackage into your project

**Step 3:** On menu bar, select Tools -> Wolffun -> CreateNetworkConfig
A network config will be created in ``Assets/ThetanWorld/Resources/ThetanSDKNetworkConfig.asset``
In config, you will have to input application id and application secret we gave you. If you don't have that, please contact our technical support.

>**Step 3.1 (Optional):** If you are in developer mode and want to use our staging environment, you can add script define "STAGING" in your Project Setting -> Player -> Script Define Symbol

>**Step 3.2 (Optional):** If you want to custom your testing endpoint, you can tick on "Use Custom Endpoint" in ThetanSDKNetworkConfig and change value of Custom Endpoint Setting

# USING GUIDE
**Step 1:** Instantiate prefab "ThetanSDK - Partner" in the package folder. After you instantiated that prefab, you **must** call [`ThetanSDKManager.Instance.Initialize(options, onDoneCallback)`](#ThetanSDKManager-Initialize). After the ``onDoneCallback`` is called, you can start using SDK.
> Note: When Initialize SDK, you can have option for handle when user have no internet, you can pass true to ``AutoShowPopupWhenLostConnection`` for SDK auto show popup when user have no internet, or you can pass false to handle it yourself.

**Step 2:** To show button Thetan World, you can call [``ThetanSDKManager.Instance.ShowButtonMainAction``](#ThetanSDKManager-ShowButtonMainAction). you can call [``ThetanSDKManager.Instance.HideButtonMainAction``](#ThetanSDKManager-HideButtonMainAction) to hide button Thetan World.

**Step 3:** Before you start the match, you should check if user is selecting any nft hero for grinding by calling ``ThetanSDKManager.Instance.IsSelectedAnyHeroNftItem``. If user select any nft hero for grinding, you can call [``ThetanSDKManager.Instance.PrepareMatchForSelectedNFT``](#ThetanSDKManager-PrepareMatchForSelectedNFT) to lock nft and prepare that nft for grinding. After [``PrepareMatchForSelectedNFT``](#ThetanSDKManager-PrepareMatchForSelectedNFT) return success, main button will become non-interactable, user can only drag them around. If you desired to turn off completely the button when user play game, you can call [``ThetanSDKManager.Instance.HideButtonMainAction``](#ThetanSDKManager-HideButtonMainAction).

**Step 4:** After you load into the game success after [``PrepareMatchForSelectedNFT``](#ThetanSDKManager-PrepareMatchForSelectedNFT), you should call [``ThetanSDKManager.Instance.StartGrindingHeroItem``](#ThetanSDKManager-StartGrindingHeroItem) to start grinding user's selected nft

>Note: `StartGrindingHeroItem` only affect on the current game session, if user somehow open game on another machine, you should check ``ThetanSDKManager.Instance.IsGrindingAnyHeroNftItem``, by then if you wish to end the grinding session, you can skip go to Step 5, or if you wish to continue grinding with previous grinding session, you have to call ``ThetanSDKManager.Instance.StartGrindingHeroItem``

>**Step 4.1 (Optional):** If your game have pause match behaviour, you can call [``ThetanSDKManager.Instance.PauseGrindingHeroItem``](#ThetanSDKManager-PauseGrindingHeroItem) to pause grinding user's nft. After that, you can call [``ThetanSDKManager.Instance.StartGrindingHeroItem``](#ThetanSDKManager-StartGrindingHeroItem) to resume grinding again.

**Step 5:** After the match end, you should call [``ThetanSDKManager.Instance.StopGrindingHeroItem``](#ThetanSDKManager-StopGrindingHeroItem) to end grinding session and unlock selected NFT.

**Step 6:** After [``ThetanSDKManager.Instance.StopGrindingHeroItem``](#ThetanSDKManager-StopGrindingHeroItem), you can call [``ThetanSDKManager.Instance.UnlockButtonMain``](#ThetanSDKManager-UnlockButtonMain) to unlock interaction with Thetan World UI again.
>Notice: this step is REQUIRED for user to start interaction with ui thetan world after grinding session.


# API
## ThetanSDKManager


---


### Initialize {#ThetanSDKManager-Initialize}

#### Declaration
``public void Initialize(SDKOption option, Action<ThetanNetworkClientState> onDoneCallback)``

#### Parameters
| Parameters | Description |
| ---------- | ----------- |
| option | an [option](#SDKOption) for configure SDK behavior|
| onDoneCallback | a callback with [ThetanNetworkClientState](#ThetanNetworkClientState) when SDK is done initialized|

#### Description
Initialize SDK before client can start using any SDK function


---


### ShowButtonMainAction {#ThetanSDKManager-ShowButtonMainAction}

#### Declaration
``public void ShowButtonMainAction()``

#### Description
Show button Thetan World for user interact with Thetan World UI


---


### HideButtonMainAction {#ThetanSDKManager-HideButtonMainAction}

#### Declaration
``public void HideButtonMainAction()``

#### Description
Completely hide button Thetan World and close all current openned thetan world UI.


---


### PrepareMatchForSelectedNFT {#ThetanSDKManager-PrepareMatchForSelectedNFT}

#### Declaration
``public void PrepareMatchForSelectedNFT(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)``

#### Parameters
| Parameters | Description |
| ---------- | ----------- |
| onSuccessCallback | callback when success prepare match with selected NFT|
| onErrorCallback | callback contain error info when prepare match is not success.|

#### Description
Lock NFT and prepare NFT for grinding session. Also, this function will lock interaction for UI Thetan World, UI can only be unlocked after you call [UnlockButtonMain](#ThetanSDKManager-UnlockButtonMain) at the end of grinding session. `onErrorCallback` will be call when cannot prepare match for NFT, error callback contain 1 of these error codes, can be access via `WolffunResponseError.Code`.
| Error Code | Description |
| ---------- | ----------- |
| WSErrorCode.ServerMaintenance | Server is under maintenance|
| [NftItemServiceErrorCode](#NftItemServiceErrorCode).NFT_IS_GRINDING_IN_ANOTHER_GAME | Selected NFT is grinding in another game|
| [NftItemServiceErrorCode](#NftItemServiceErrorCode).ANOTHER_NFT_IS_GRINDING | Other NFT is grinding in your game|
| [NftItemServiceErrorCode](#NftItemServiceErrorCode).HERO_MAX_GRIND_STAGE | Hero NFT is reached limit grind stage and cannot be grinded anymore|
| [NftItemServiceErrorCode](#NftItemServiceErrorCode).NFT_DAILY_LIMIT_REACH | Selected NFT is reached daily grind time limit|
| [NftItemServiceErrorCode](#NftItemServiceErrorCode).USER_NOT_OWN_NFT | Selected NFT is not belong to user, maybe user has already sold this NFT|
| Other | Unknown Error |


---


### PrepareMatchForSelectedNFTAutoHandleError {#ThetanSDKManager-PrepareMatchForSelectedNFTAutoHandleError}

#### Declaration
``public void PrepareMatchForSelectedNFTAutoHandleError(Action<NftItemServiceErrorCode> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)``

#### Parameters
| Parameters | Description |
| ---------- | ----------- |
| onSuccessCallback | callback when call prepare match for NFT success or there is error but user confirm start match without grind NFT. |
| onErrorCallback | callback when call prepare grind has error and user confirm to go back.|

#### Description
Lock NFT and prepare NFT for grinding session. This function is similar to [PrepareMatchForSelectedNFT](#ThetanSDKManager-PrepareMatchForSelectedNFT) but it auto show error message and ask user if user want to continue playing without grinding when there is an error.
- With onSuccessCallback, you can check if it actual success by check callback value equal to NftItemServiceErrorCode.Success. Otherwise there is an error but user accept to continue, in this case, the callback value contain ErrorCode for error. Its error value is the same as [PrepareMatchForSelectedNFT](#ThetanSDKManager-PrepareMatchForSelectedNFT).
- With onErrorCallback, that mean there is an error and user confirm to go back. The error callback value is the same as [PrepareMatchForSelectedNFT](#ThetanSDKManager-PrepareMatchForSelectedNFT).



---


### StartGrindingHeroItem {#ThetanSDKManager-StartGrindingHeroItem}

#### Declaration
``public void StartGrindingHeroItem()``

#### Description
Start grinding NFT after prepare match for NFT is succedd. If call this before call PrepareMatchForSelectedNFT, it won't do anything. This function also can be used to resume grinding after call [PauseGrindingHeroItem](#ThetanSDKManager-PauseGrindingHeroItem)


---


### PauseGrindingHeroItem {#ThetanSDKManager-PauseGrindingHeroItem}

#### Declaration
``public void PauseGrindingHeroItem()``

#### Description
Temporary pause grinding hero NFT after called [StartGrindingHeroItem](#ThetanSDKManager-StartGrindingHeroItem). Used for case when your game is paused and you need to pause grinding too.


---


### StopGrindingHeroItem {#ThetanSDKManager-StopGrindingHeroItem}

#### Declaration
``public void StopGrindingHeroItem()``

#### Description
End grinding session and unlock selected NFT. You have to call this at the end of the game to unlock NFT and stop grinding session. Otherwise, user cannot select and grind other NFT.


---


### UnlockButtonMain {#ThetanSDKManager-UnlockButtonMain}

#### Declaration
``public void UnlockButtonMain()``

#### Description
Unlock Interaction for UI Thetan World. You may call this right after [StopGrindingHeroItem](#ThetanSDKManager-StopGrindingHeroItem) or anytime that suit your game flow but it must be called after `StopGrindingHeroItem`. Otherwise, user cannot interact with UI Thetan World.


---


### LogOut {#ThetanSDKManager-LogOut}

#### Declaration
``public void LogOut()``

#### Description
Log out current account.


---


### SetMatchMaxDuration {#ThetanSDKManager-SetMatchMaxDuration}

#### Declaration
`public void SetMatchMaxDuration(int matchMaxDuration)`

#### Parameters
| Parameters | Description |
| ---      | ----------------- |
| matchMaxDuration | match max duration time out (count in seconds) to unlock NFT and end grinding session when receive no grinding signal from game client|

#### Description
Set max grinding session time out. Each 5 seconds game client will ping server to notify grinding, if server cannot receive grinding signal for max grinding session time out, server will automatically end grinding session and unlock NFT. Default value is 300 seconds.


---


### CheckHeroIsSelected {#ThetanSDKManager-CheckHeroIsSelected}

#### Declaration
`public bool CheckHeroIsSelected(string heroId)`

#### Parameters
| Parameters | Description |
| ---      | ----------------- |
| heroId | hero NFT id need to check|

#### Returns
`bool`: Return true if user is selecting NFT with `heroId`. Otherwise, return false

#### Description
Check if user is selecting hero NFT with id `heroId`


---


### RefreshHeroNftData {#ThetanSDKManager-RefreshHeroNftData}

#### Declaration
`public void RefreshHeroNftData(string nftId, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)`

#### Parameters
| Parameters | Description |
| ---      | ----------------- |
| heroId | hero NFT id need to check |
| onSuccessCallback |  callback when success refresh data, return new [HeroNftItem](#HeroNftItem) data|
| onErrorCallback |  error callback when refresh data fail|


#### Description
Call server to get new data of hero NFT with `nftId`


---


### RegisterOnChangeSelectedHeroNft {#ThetanSDKManager-RegisterOnChangeSelectedHeroNft}

#### Declaration
`public void RegisterOnChangeSelectedHeroNft(Action<string> callback)`

#### Parameters
| Parameters | Description |
| ---      | ----------------- |
| callback | A callback with heroNFTId when user select new hero NFT. This callback also be invoked with string.empty or null when user unselect hero NFT |


#### Description
Register callback when user select/unselect an hero NFT


---


### UnRegisterOnChangeSelectedHeroNft {#ThetanSDKManager-UnRegisterOnChangeSelectedHeroNft}

#### Declaration
`public void UnRegisterOnChangeSelectedHeroNft(Action<string> callback)`

#### Parameters
| Parameters | Description |
| ---      | ----------------- |
| callback | A callback used when [RegisterOnChangeSelectedHeroNft](#ThetanSDKManager-RegisterOnChangeSelectedHeroNft) |


#### Description
UnRegister callback when user select/unselect an hero NFT


---


### Properties
| Properties | Description |
| ---      | ----------------- |
| SelectedHeroNftId | Get selected hero NFT id|
| IsSelectedAnyHeroNftItem | Check is selected any hero NFT|
| GrindingHeroNftId | Get grinding hero NFT id|
| IsGrindingAnyHeroNftItem | Check is grinding any hero NFT|


---


### Public Event
| Properties | Description |
| ---      | ----------------- |
| OnUserLogOutCallback | Callback when user log out |
| OnChangeNetworkClientState | Callback when client changed its internal [client state](#ThetanNetworkClientState) |
| OnOpenMainUI | Callback when user open main Thetan World UI |
| OnCloseMainUI | Callback when Thetan World UI is closed|


---


# Data Model





### SDKOption {#SDKOption}
#### Description
SDK option for configure SDK behavior. This option is passed into SDK when initialized.

#### Properties
| Properties | Description |
| ---      | ----------------- |
| AutoShowPopupWhenLostConnection | Allow SDK auto show popup lost connection when user is not connected to network|
| UseFullscreenLogin | Make SDK use full screen or popup login UI|


---


### ThetanNetworkClientState {#ThetanNetworkClientState}
#### Description
An enum descript current client state, can use this to determine whether is user logged in, or is connected to network?

#### Properties
| Properties | Description |
| ---      | ----------------- |
| NotInitialized | Client is not initialized yet, must call [Initialize](#ThetanSDKManager-Initialize)|
| NotLoggedIn | User is not logged in|
| NotLoggedInNoNetwork | User is not logged in and not connected to network|
| LoggedIn | User is logged in|
| LoggedInNoNetwork | User is logged in but temporary not connected to network|
| Banned | User is banned|


---


### HeroNftItem {#HeroNftItem}
#### Description
Struct use for holding hero nft item data

#### Properties
| Properties | Description |
| ---      | ----------------- |
| id | NFT id |
| [ingameInfo](#NftIngameInfo) | Info use by ingame for nft |
| metaData | [Metadata](#HeroNftMetaData) of this NFT |
| equipmentSet | A dictionary that contain info for all equipment slot inside hero NFT item. Key is [EquipmentItemType](#EquipmentItemType). Value is [NFTEquipmentInfo](#NFTEquipmentInfo)|
| [grindInfo](#CommonGrindInfo) | Contain info about this NFT grind ability, rewards, speed, stage, ... |
| marketInfo | A [MarketInfo](#MarketInfo) of this NFT|
| onchainInfo | An [OnChainInfo](#OnChainInfo) of this NFT |


---


### NftIngameInfo {#NftIngameInfo}
#### Description
Struct use for holding hero nft item data

#### Properties
| Properties | Description |
| ---      | ----------------- |
| gameId | [Game World Type](#GameWorldType) of NFT |
| ingameID | Ingame item id (Only used in specific game world that NFT is belong to) |
| rarity | Rarity of this NFT |
| kind | [Item kind](#ItemKind) of this item |
| type | typeId of this item |


---


### HeroNftMetaData {#HeroNftMetaData}
#### Description
Meta data for descript Hero NFT

#### Properties
| Properties | Description |
| ---      | ----------------- |
| name | Display name of this NFT |


---


### EquipmentItemType {#EquipmentItemType}
#### Description
Enum defying all equipment type that can equiped on hero NFT

#### Properties
| Properties | Description |
| ---      | ----------------- |
| Hat | 1 |
| Shirt | 2 |
| Glove | 3 |
| Pant | 4 |
| Shoe | 5 |
| Weapon | 6 |


---


### NFTEquipmentInfo {#NFTEquipmentInfo}
#### Description
Contain infomation about an equipment slot inside hero nft item

#### Properties
| Properties | Description |
| ---      | ----------------- |
| requiredTypeId | Require Equipment Type Id |
| equippedId | ItemId of equipment item that been equipped in this slot. If this slot is not equipped yet, this field is null or empty |


---


### CommonGrindInfo {#CommonGrindInfo}
#### Description
Contain common grind info of hero NFT item

#### Properties
| Properties | Description |
| ---      | ----------------- |
| status | Contain grind session info of this NFT. If this NFT is not being grinded, this value is null |
| grindPoint | Total reward this NFT has earned through grinding |
| currentGrindSpeed | Current grind speed of this NFT (THG/s) |
| maxGrindSpeed | Maximum grind speed this NFT can reach (THG/s) |
| grindAbility | The grind ability of this NFT (value range [0, 1]) |
| stage | Current stage of this NFT |
| maxStage | Maximum stage of this NFT |
| reachMaxStage | Is this NFT reached its maximum stage and cannot be grinded anymore |


---


### MarketInfo {#MarketInfo}
#### Description
Contain market info of this NFT

#### Properties
| Properties | Description |
| ---      | ----------------- |
| status | A string contain status of this NFT. If this NFT is not in any market's transaction, this value will be null or empty. |


---


### OnChainInfo {#OnChainInfo}
#### Description
Contain Chain info of NFT

#### Properties
| Properties | Description |
| ---      | ----------------- |
| tokenId | A string contain token id of NFT on chain |


---


### GrindStatus {#GrindStatus}
#### Description
Contain grind session info

#### Properties
| Properties | Description |
| ---      | ----------------- |
| appId | AppId of app this NFT is grinding |
| grindId | Grind session ID |
| startGrindAt | DateTime of when this NFT is started grind. |
| timeOut | DateTime of when this NFT is timeout grind when server cannot receive grind signal. |

---


### GameWorldType {#GameWorldType}
#### Description
Enum defying all Thetan Games

#### Properties
| Properties | Description |
| ---      | ----------------- |
| ThetanArena | 0 |
| ThetanRivals | 1 |
| ThetanUGC | 2 |
| ThetanImmortal | 3 |
| ThetanMarket | 4 |


---


### NFTRarity {#NFTRarity}
#### Description
Enum defying all NFT rarity

#### Properties
| Properties | Description |
| ---      | ----------------- |
| Common | 1 |
| Rare | 2 |
| Epic | 3 |
| Legend | 4 |


---


### ItemKind {#ItemKind}
#### Description
Enum defying item kind of NFT (Hero, Equipment, Ticket, ....)

#### Properties, Method or Contructor
| Properties | Description |
| ---      | ----------------- |
| Hero | 5 |


---


### NftItemServiceErrorCode {#NftItemServiceErrorCode}
#### Description
Enum defying ErrorCode when interact with NFT ITEM API

#### Properties, Method or Contructor
| Properties | Description |
| ---      | ----------------- |
| SDK_VERSION_NOT_SUPPORTED | This SDK version is not supported by server anymore, please update your SDK |
| NOT_LOGGED_IN | Client is not in logged in state |
| SUCCESS | API Success, there is no error |
| NETWORK_ERROR | There is network error when making API request |
| NFT_NOT_MINT | NFT is not minted yet, please go to marketplace to mint NFT|
| USER_NOT_OWN_NFT | User not the owner of request NFT |
| ANOTHER_NFT_IS_GRINDING | Another NFT is grinding in this game that prevent API request |
| NFT_IS_GRINDING_IN_ANOTHER_GAME | Selected NFT is grinding in another game that prevent API request |
| NFT_DAILY_LIMIT_REACH | Selected NFT is reached its daily grind limit |
| HERO_NOT_GRINDING | Selected hero is not grinding |
| HERO_MAX_GRIND_STAGE | Selected hero is at maximum grind stage and cannot be grinded anymore |
| UNKNOWN | Unknown Error |

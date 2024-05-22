# Thetan World SDK

This SDK allow player to select NFT, and bring NFT into game to play and earn THG. The process user play and earn THG through playing an NFT is called **Grind**

Grind process with be splitted into phases that depend on your game to decide when to invoke which phase. Each phase is descript as following:
- **Select NFT**: User can select NFT through Thetan World UI which is integrated inside SDK. After select NFT success, user can start grind NFT.
- **Prepare Grind**: This phase is called by your game to lock selected NFT and start a grind session. User cannot unselect NFT or select other NFT. User also cannot call prepare for another NFT when there is one NFT is locked for grind.
- **Start Grind**: This phase is called by your game to start grind for NFT. Grinding will be count every 5 second tick.
- **Pause Grind**: This phase is optional, you can call this phase when your game mechanic support pause game when playing or something similar to that. You can resume grinding by calling Start Grind again.
- **End Grind**: This phase is called by your game to end grinding session and unlock selected NFT.
- **Unlock UI Thetan World**: This phase is called by your game to unlock Thetan World UI interaction after grinding session is completed.

## Table of Contents

- [Installation](#installation)
- [Using Guide](#using-guide)
- [API](#api)
- [Data Model](#data-model)

# Installation

### **Step 1:** Add these packages into your project
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


### **Step 2**: Import SDK
Download [ThetanWorldSDK.unitypackage](https://github.com/WolffunService/thetan-world-sdk-docs/blob/main/ThetanWorldSDK.unitypackage) and import it into your project


### **Step 3:** CreateNetworkConfig
On menu bar, select Tools -> Wolffun -> CreateNetworkConfig
A network config will be created in ``Assets/ThetanWorld/Resources/ThetanSDKNetworkConfig.asset``
In config, you will have to input application id and application secret we gave you. If you don't have that, please contact our technical support.

>**Step 3.1 (Optional):** If you are in developer mode and want to use our staging environment, you can add script define "STAGING" in your Project Setting -> Player -> Script Define Symbol

>**Step 3.2 (Optional):** If you want to custom your testing endpoint, you can tick on "Use Custom Endpoint" in ThetanSDKNetworkConfig and change value of Custom Endpoint Setting

# Using Guide

### **Step 1**: Initialize
Instantiate prefab "ThetanSDK - Partner" in the package folder. After you instantiated that prefab, you **must** call [`ThetanSDKManager.Instance.Initialize(options, onDoneCallback)`](#initialize). After the ``onDoneCallback`` is called, you can start using SDK.
> Note: When Initialize SDK, you can have option for handle when user have no internet, you can pass true to ``AutoShowPopupWhenLostConnection`` for SDK auto show popup when user have no internet, or you can pass false to handle it yourself.

### **Step 2**: Show Main Button 
To show button Thetan World, you can call [``ThetanSDKManager.Instance.ShowButtonMainAction``](#showbuttonmainaction). you can call [``ThetanSDKManager.Instance.HideButtonMainAction``](#hidebuttonmainaction) to hide button Thetan World.

### **Step 3:** Prepare to start match
Before you start the match, you should check if user is selecting any nft hero for grinding by calling ``ThetanSDKManager.Instance.IsSelectedAnyHeroNftItem``. If user select any nft hero for grinding, you can call [``ThetanSDKManager.Instance.PrepareMatchForSelectedNFT``](#preparematchforselectednft) to lock nft and prepare that nft for grinding. After [``PrepareMatchForSelectedNFT``](#preparematchforselectednft) return success, main button will become non-interactable, user can only drag them around. If you desired to turn off completely the button when user play game, you can call [``ThetanSDKManager.Instance.HideButtonMainAction``](#hidebuttonmainaction).

### **Step 4:** Start Grinding
After you load into the game success after [``PrepareMatchForSelectedNFT``](#preparematchforselectednft), you should call [``ThetanSDKManager.Instance.StartGrindingHeroItem``](#startgrindingheroitem) to start grinding user's selected nft

>Note: `StartGrindingHeroItem` only affect on the current game session, if user somehow open game on another machine, you should check ``ThetanSDKManager.Instance.IsGrindingAnyHeroNftItem``, by then if you wish to end the grinding session, you can skip go to Step 5, or if you wish to continue grinding with previous grinding session, you have to call ``ThetanSDKManager.Instance.StartGrindingHeroItem``

>**Step 4.1 (Optional):** If your game have pause match behaviour, you can call [``ThetanSDKManager.Instance.PauseGrindingHeroItem``](#pausegrindingheroitem) to pause grinding user's nft. After that, you can call [``ThetanSDKManager.Instance.StartGrindingHeroItem``](#startgrindingheroitem) to resume grinding again.

### **Step 5:** Stop Grinding
After the match end, you should call [``ThetanSDKManager.Instance.StopGrindingHeroItem``](#stopgrindingheroitem) to end grinding session and unlock selected NFT.

### **Step 6:** Unlock Main Button
After [``ThetanSDKManager.Instance.StopGrindingHeroItem``](#stopgrindingheroitem), you can call [``ThetanSDKManager.Instance.UnlockButtonMain``](#unlockbuttonmain) to unlock interaction with Thetan World UI again.
>Notice: this step is REQUIRED for user to start interaction with ui thetan world after grinding session.


# API
## ThetanSDKManager


---


### Initialize

**Declaration:**:
``public void Initialize(SDKOption option, Action<ThetanNetworkClientState> onDoneCallback)``

| Parameters | Description |
| ---------- | ----------- |
| option | an [option](#sdkoption) for configure SDK behavior|
| onDoneCallback | a callback with [ThetanNetworkClientState](#thetannetworkclientstate) when SDK is done initialized|

**Description**:
Initialize SDK before client can start using any SDK function


---


### ShowButtonMainAction

**Declaratio:**
``public void ShowButtonMainAction()``

**Description**: Show button Thetan World for user interact with Thetan World UI


---


### HideButtonMainAction

**Declaration:**
``public void HideButtonMainAction()``

**Description**: Completely hide button Thetan World and close all current openned thetan world UI.


---


### PrepareMatchForSelectedNFT

**Declaration:**
``public void PrepareMatchForSelectedNFT(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)``

| Parameters | Description |
| ---------- | ----------- |
| onSuccessCallback | callback when success prepare match with selected NFT|
| onErrorCallback | callback contain error info when prepare match is not success.|

**Description**: Lock NFT and prepare NFT for grinding session. Also, this function will lock interaction for UI Thetan World, UI can only be unlocked after you call [UnlockButtonMain](#unlockbuttonmain) at the end of grinding session. `onErrorCallback` will be call when cannot prepare match for NFT, error callback contain 1 of these error codes, can be access via `WolffunResponseError.Code`.

| Error Code | Description |
| ---------- | ----------- |
| WSErrorCode.ServerMaintenance | Server is under maintenance|
| [NftItemServiceErrorCode](#nftitemserviceerrorcode).NFT_IS_GRINDING_IN_ANOTHER_GAME | Selected NFT is grinding in another game|
| [NftItemServiceErrorCode](#nftitemserviceerrorcode).ANOTHER_NFT_IS_GRINDING | Other NFT is grinding in your game|
| [NftItemServiceErrorCode](#nftitemserviceerrorcode).HERO_MAX_GRIND_STAGE | Hero NFT is reached limit grind stage and cannot be grinded anymore|
| [NftItemServiceErrorCode](#nftitemserviceerrorcode).NFT_DAILY_LIMIT_REACH | Selected NFT is reached daily grind time limit|
| [NftItemServiceErrorCode](#nftitemserviceerrorcode).USER_NOT_OWN_NFT | Selected NFT is not belong to user, maybe user has already sold this NFT|
| Other | Unknown Error |


---


### PrepareMatchForSelectedNFTAutoHandleError

**Declaration:**
``public void PrepareMatchForSelectedNFTAutoHandleError(Action<NftItemServiceErrorCode> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)``

| Parameters | Description |
| ---------- | ----------- |
| onSuccessCallback | callback when call prepare match for NFT success or there is error but user confirm start match without grind NFT. |
| onErrorCallback | callback when call prepare grind has error and user confirm to go back.|

**Description**: Lock NFT and prepare NFT for grinding session. This function is similar to [PrepareMatchForSelectedNFT](#preparematchforselectednft) but it auto show error message and ask user if user want to continue playing without grinding when there is an error.
- With onSuccessCallback, you can check if it actual success by check callback value equal to NftItemServiceErrorCode.Success. Otherwise there is an error but user accept to continue, in this case, the callback value contain ErrorCode for error. Its error value is the same as [PrepareMatchForSelectedNFT](#preparematchforselectednft).
- With onErrorCallback, that mean there is an error and user confirm to go back. The error callback value is the same as [PrepareMatchForSelectedNFT](#preparematchforselectednft).



---


### StartGrindingHeroItem

**Declaration:**
``public void StartGrindingHeroItem()``

**Description:** Start grinding NFT after prepare match for NFT is succedd. If call this before call PrepareMatchForSelectedNFT, it won't do anything. This function also can be used to resume grinding after call [PauseGrindingHeroItem](#pausegrindingheroitem)


---


### PauseGrindingHeroItem

**Declaration:**
``public void PauseGrindingHeroItem()``

**Description:** Temporary pause grinding hero NFT after called [StartGrindingHeroItem](#startgrindingheroitem). Used for case when your game is paused and you need to pause grinding too.


---


### StopGrindingHeroItem

**Declaration**
``public void StopGrindingHeroItem()``

**Description:** End grinding session and unlock selected NFT. You have to call this at the end of the game to unlock NFT and stop grinding session. Otherwise, user cannot select and grind other NFT.


---


### UnlockButtonMain

**Declaration**
``public void UnlockButtonMain()``

**Description:** Unlock Interaction for UI Thetan World. You may call this right after [StopGrindingHeroItem](#stopgrindingheroitem) or anytime that suit your game flow but it must be called after `StopGrindingHeroItem`. Otherwise, user cannot interact with UI Thetan World.


---


### LogOut

**Declaration**
``public void LogOut()``

**Description:** Log out current account.


---


### SetMatchMaxDuration

**Declaration**
`public void SetMatchMaxDuration(int matchMaxDuration)`

| Parameters | Description |
| ---      | ----------------- |
| matchMaxDuration | match max duration time out (count in seconds) to unlock NFT and end grinding session when receive no grinding signal from game client|

**Description:** Set max grinding session time out. Each 5 seconds game client will ping server to notify grinding, if server cannot receive grinding signal for max grinding session time out, server will automatically end grinding session and unlock NFT. Default value is 300 seconds.


---


### CheckHeroIsSelected

**Declaration**
`public bool CheckHeroIsSelected(string heroId)`

| Parameters | Description |
| ---      | ----------------- |
| heroId | hero NFT id need to check|

**Returns**

bool: Return `true` if user is selecting NFT with `heroId`. Otherwise, return `false`

**Description:**
Check if user is selecting hero NFT with id `heroId`


---


### RefreshHeroNftData

**Declaration:**
`public void RefreshHeroNftData(string nftId, Action<HeroNftItem> onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)`

| Parameters | Description |
| ---      | ----------------- |
| heroId | hero NFT id need to check |
| onSuccessCallback |  callback when success refresh data, return new [HeroNftItem](#heronftitem) data|
| onErrorCallback |  error callback when refresh data fail|


**Description:**
Call server to get new data of hero NFT with `nftId`


---


### RegisterOnChangeSelectedHeroNft

**Declaration:**
`public void RegisterOnChangeSelectedHeroNft(Action<string> callback)`

| Parameters | Description |
| ---      | ----------------- |
| callback | A callback with heroNFTId when user select new hero NFT. This callback also be invoked with string.empty or null when user unselect hero NFT |


**Description:**
Register callback when user select/unselect an hero NFT


---


### UnRegisterOnChangeSelectedHeroNft

**Declaration:**
`public void UnRegisterOnChangeSelectedHeroNft(Action<string> callback)`

| Parameters | Description |
| ---      | ----------------- |
| callback | A callback used when [RegisterOnChangeSelectedHeroNft](#registeronchangeselectedheronft) |


**Description**
UnRegister callback when user select/unselect an hero NFT


---


| Properties | Description |
| ---      | ----------------- |
| SelectedHeroNftId | Get selected hero NFT id|
| IsSelectedAnyHeroNftItem | Check is selected any hero NFT|
| GrindingHeroNftId | Get grinding hero NFT id|
| IsGrindingAnyHeroNftItem | Check is grinding any hero NFT|


---



| Public Event | Description |
| ---      | ----------------- |
| OnUserLogOutCallback | Callback when user log out |
| OnChangeNetworkClientState | Callback when client changed its internal [client state](#thetannetworkclientstate) |
| OnOpenMainUI | Callback when user open main Thetan World UI |
| OnCloseMainUI | Callback when Thetan World UI is closed|


---



# Data Model


### SDKOption
**Description:**
SDK option for configure SDK behavior. This option is passed into SDK when initialized.

| Properties | Description |
| ---      | ----------------- |
| AutoShowPopupWhenLostConnection | Allow SDK auto show popup lost connection when user is not connected to network|
| UseFullscreenLogin | Make SDK use full screen or popup login UI|


---


### ThetanNetworkClientState
**Description:**
An enum descript current client state, can use this to determine whether is user logged in, or is connected to network?

| Properties | Description |
| ---      | ----------------- |
| NotInitialized | Client is not initialized yet, must call [Initialize](#initialize)|
| NotLoggedIn | User is not logged in|
| NotLoggedInNoNetwork | User is not logged in and not connected to network|
| LoggedIn | User is logged in|
| LoggedInNoNetwork | User is logged in but temporary not connected to network|
| Banned | User is banned|


---


### HeroNftItem
**Description:**
Struct use for holding hero nft item data

| Properties | Description |
| ---      | ----------------- |
| id | NFT id |
| [ingameInfo](#nftingameinfo) | Info use by ingame for nft |
| metaData | [Metadata](#heronftmetadata) of this NFT |
| equipmentSet | A dictionary that contain info for all equipment slot inside hero NFT item. Key is [EquipmentItemType](#equipmentitemtype). Value is [NFTEquipmentInfo](#nftequipmentinfo)|
| [grindInfo](#commongrindinfo) | Contain info about this NFT grind ability, rewards, speed, stage, ... |
| marketInfo | A [MarketInfo](#marketinfo) of this NFT|
| onchainInfo | An [OnChainInfo](#onchaininfo) of this NFT |


---


### NftIngameInfo
**Description:**
Struct use for holding hero nft item data

| Properties | Description |
| ---      | ----------------- |
| gameId | [Game World Type](#gameworldtype) of NFT |
| ingameID | Ingame item id (Only used in specific game world that NFT is belong to) |
| rarity | Rarity of this NFT |
| kind | [Item kind](#itemkind) of this item |
| type | typeId of this item |


---


### HeroNftMetaData
**Description:**
Meta data for descript Hero NFT

| Properties | Description |
| ---      | ----------------- |
| name | Display name of this NFT |


---


### EquipmentItemType
**Description:**
Enum defying all equipment type that can equiped on hero NFT

| Properties | Description |
| ---      | ----------------- |
| Hat | 1 |
| Shirt | 2 |
| Glove | 3 |
| Pant | 4 |
| Shoe | 5 |
| Weapon | 6 |


---


### NFTEquipmentInfo
**Description:**
Contain infomation about an equipment slot inside hero nft item

| Properties | Description |
| ---      | ----------------- |
| requiredTypeId | Require Equipment Type Id |
| equippedId | ItemId of equipment item that been equipped in this slot. If this slot is not equipped yet, this field is null or empty |


---


### CommonGrindInfo
**Description:**
Contain common grind info of hero NFT item

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


### MarketInfo
**Description:**
Contain market info of this NFT

| Properties | Description |
| ---      | ----------------- |
| status | A string contain status of this NFT. If this NFT is not in any market's transaction, this value will be null or empty. |


---


### OnChainInfo
**Description:**
Contain Chain info of NFT

| Properties | Description |
| ---      | ----------------- |
| tokenId | A string contain token id of NFT on chain |


---


### GrindStatus
**Description:**
Contain grind session info

| Properties | Description |
| ---      | ----------------- |
| appId | AppId of app this NFT is grinding |
| grindId | Grind session ID |
| startGrindAt | DateTime of when this NFT is started grind. |
| timeOut | DateTime of when this NFT is timeout grind when server cannot receive grind signal. |

---


### GameWorldType
**Description:**
Enum defying all Thetan Games

| Properties | Description |
| ---      | ----------------- |
| ThetanArena | 0 |
| ThetanRivals | 1 |
| ThetanUGC | 2 |
| ThetanImmortal | 3 |
| ThetanMarket | 4 |


---


### NFTRarity
**Description:**
Enum defying all NFT rarity

| Properties | Description |
| ---      | ----------------- |
| Common | 1 |
| Rare | 2 |
| Epic | 3 |
| Legend | 4 |


---


### ItemKind
**Description:**
Enum defying item kind of NFT (Hero, Equipment, Ticket, ....)

#### Properties, Method or Contructor
| Properties | Description |
| ---      | ----------------- |
| Hero | 5 |


---


### NftItemServiceErrorCode
**Description:**
Enum defying ErrorCode when interact with NFT ITEM API

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

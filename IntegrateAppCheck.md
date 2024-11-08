# Intergrate Firebase AppCheck


> [!IMPORTANT]
> Firebase AppCheck is REQUIRED to verify authorized game client to access our services

## Step 1: Import Firebase AppCheck into your project
Follow https://firebase.google.com/docs/app-check/unity/default-providers to setup Firebase Appcheck in your project

## Step 2: Get AppCheckToken then Set to SDK

Set appCheck proviver when init

```csharp
    protected void InitFirebase()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        FirebaseAppCheck.SetAppCheckProviderFactory(PlayIntegrityProviderFactory.Instance);
#endif
#if UNITY_IOS && !UNITY_EDITOR
        FirebaseAppCheck.SetAppCheckProviderFactory(AppAttestProviderFactory.Instance);
#endif
    }
```

Get appCheck token and set to ThetanSDKManager
```csharp
    public async void GetAppCheck()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        Debug.Log("Start GetAppCheckTokenAsync");
        try
        {
            var firebaseApp = FirebaseAppCheck.DefaultInstance;
            if (firebaseApp == null) return;
            var appCheckToken = await firebaseApp.GetAppCheckTokenAsync(false);
            ThetanSDKManager.Instance.SetAppCheckToken(appCheckToken.Token);
        }
        catch (AggregateException e)
        {
            Debug.LogError($"GetAppCheckTokenAsync default instance failed with exception: {e.Message}");
        }
#endif
    }
```
## Step 3: Active AppCheck in Firebase
Go to Firebase -> AppCheck: https://console.firebase.google.com/project/your-project-id/appcheck 
#### Setting Android with Play Intergrity
![Setting with Play Integrity](docs/images/firebase-appcheck-android.png)

#### Setting iOS with App Attest
![Setting with App Attest](docs/images/firebase-appcheck-ios.png)

## Step 4: Create Service Account and Send to us
Go to GCP Service Account: https://console.cloud.google.com/iam-admin/serviceaccounts?project=your-project-id

#### Create service account with name: thetanworld-sdk-appcheck
![Create service account](docs/images/service-account-create.png)

#### Grant role: Firebase App Check Token Verifier
![Grant role](docs/images/service-account-grantrole.png)

### Create key and send to us
![Create key](docs/images/service-account-create-key.png)

Finally send us that key, thanks so much!
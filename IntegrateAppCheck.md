# Intergrate Firebase AppCheck


> [!IMPORTANT]
> Firebase AppCheck is REQUIRED to verify authorized game client to access our services

## Step 1: Import Firebase AppCheck into your project
Follow https://firebase.google.com/docs/app-check/unity/default-providers to setup Firebase Appcheck in your project

## Step 2: Get AppCheckToken then Set to SDK

Set appCheck proviver when init
![Init Provider](docs/images/appcheck-provider-init.png)

Get appCheck token and set to ThetanSDKManager
![Get AppCheck Token](docs/images/get-app-check.png)

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
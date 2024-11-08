using Firebase.Extensions;
using Firebase;
using Firebase.AppCheck;
using Wolffun.Log;
using Wolffun.MultiPlayer;

public partial class FirebaseManager : MonoSingleton<FirebaseManager>
{
    #region Init
    private bool isFirebaseInit = false;

    protected void InitFirebase()
    {
        CommonLog.Log("Init database");
        isFirebaseInit = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        FirebaseAppCheck.SetAppCheckProviderFactory(PlayIntegrityProviderFactory.Instance);
#endif
#if UNITY_IOS && !UNITY_EDITOR
        FirebaseAppCheck.SetAppCheckProviderFactory(AppAttestProviderFactory.Instance);
#endif
#if UNITY_ANDROID || UNITY_IOS

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // where app is a Firebase.FirebaseApp property of your application class.

                // Set a flag here indicating that Firebase is ready to use by your
                // application.
                InitialFirebase();
                isFirebaseInit = true;
                CommonLog.Log("Firebase is ready to use");
            }
            else
            {
                CommonLog.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
#endif
    }

    private void InitialFirebase()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        GoogleIntegrityController.PrepareIntegrityToken().Forget();
#endif
        
        GetAppCheck();

    }
    #endregion
    
    #region Mono funtions
    private void Start()
    {
        InitFirebase();
    }
#endregion
}

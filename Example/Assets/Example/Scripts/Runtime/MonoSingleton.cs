
using UnityEngine;

/// <summary>
///     Be aware this will not prevent a non singleton constructor
///     such as `T myT = new T();`
///     To prevent that, add `protected T () {}` to your singleton class.
///     As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour
    where T : MonoSingleton<T>
{
    #region Static Fields and Constants

    private static T instance;

    private static readonly object ThreadLock = new object();

    protected static bool isInitialized;
    private static bool _alive = true;

    /// <summary>
    /// Được tự động tạo ra khi không tìm thấy instance trên scene 
    /// Nếu có phát hiện duplicate thì nên xóa thằng có trước nếu isAutoCreate = true
    /// </summary>
    private static bool isAutoCreate = false;
    #endregion

    #region Proprieties

    #region Public Proprieties

    public static T Instance
    {
        get
        {
            lock (ThreadLock)
            {
                if (instance != null)
                    return instance;

                var objects = FindObjectsOfType<T>();
                if(objects != null)
                {
                    switch (objects.Length)
                    {
                        case 1:
                            instance = objects[0];
                            break;
                        case > 1:
                        {
                            Debug.LogError("Something went really wrong - there should never be more than 1 singleton!" +
                                           " Reopening the scene might fix it." + objects[0].gameObject.name + " | " + objects[1].gameObject.name);
                            foreach (var manager in objects)
                                Destroy(manager.gameObject);
                            break;
                        }
                    }
                }

                //instance = objects.Length > 0 ? objects[0] : FindObjectOfType<T>();

                if (instance == null)
                {
                    var singleton = new GameObject(typeof(T).Name + " (Singleton)");
                    instance = singleton.AddComponent<T>();
                    isInitialized = false;
                    isAutoCreate = true;
                    Debug.LogWarning("An instance of " + typeof(T) + " is needed in the scene, so '" + singleton +
                              "' was created with DontDestroyOnLoad.");
                }

                if (isInitialized)
                {
                    return instance;
                }

                instance.Initialize();
                isInitialized = true;
                _alive = true;
                return instance;
            }
        }
    }

    #endregion

    #endregion

    #region Methods

    #region Monobehaviour Methods

    /// <summary>
    ///     When Unity quits, it destroys objects in a random order.
    ///     In principle, a Singleton is only destroyed when application quits.
    ///     If any script calls Instance after it have been destroyed,
    ///     it will create a buggy ghost object that will stay on the Editor scene
    ///     even after stopping playing the Application. Really bad!
    ///     So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    protected virtual void OnDestroy()
    {
        instance = null;
        isInitialized = false;
        _alive = false;
    }

    #endregion

    #region Protected Methods

    protected virtual void Awake()
    {
        if(instance != null && instance != this)
        {
            if(isAutoCreate)
            {
                Debug.LogError("Duplicate and auto create " + typeof(T) + " - destroy old: " + instance.name);
                DestroyImmediate(instance.gameObject);
                instance = null;
                isAutoCreate = false;
            } else
            {
                Debug.LogError("Duplicate and not create " + typeof(T) + " - destroy new: " + this.name);
                DestroyImmediate(this.gameObject);
            }
        }
        if (Instance) { }
        _alive = true;
    }

    protected virtual void Initialize()
    {
    }

    public static bool IsAlive
    {
        get
        {
            if (instance == null)
                return false;
            return _alive;
        }
    }
    #endregion

    #region Private Methods

    private void OnApplicationQuit()
    {
        Debug.LogWarning("(Singleton) OnApplicationQuit");

        _alive = false;
        instance = null;
    }


    #endregion

    #endregion
}

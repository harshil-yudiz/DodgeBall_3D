using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : Component
{
    public bool DonotDestroyOnLoad = false;

    public static T Instance { get; private set; }
    public virtual void Awake()
    {
        Instance = this as T;
        if (DonotDestroyOnLoad)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}

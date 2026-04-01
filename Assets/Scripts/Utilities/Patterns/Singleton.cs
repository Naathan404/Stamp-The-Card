using Fusion;
using UnityEngine;

public class Singleton<T> : NetworkBehaviour where T : class
{
    public static T Instance;
    protected void Awake()
    {
        if(Instance != null && Instance != this as T)
        {
           Destroy(gameObject);
           return; 
        }
        Instance = this as T;
    }
}

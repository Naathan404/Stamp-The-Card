using Fusion;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : class
{
    public static T Instance;
    protected void Awake()
    {
        if(Instance != null && Instance != this as T)
        {
           Destroy(this.gameObject);
           return; 
        }
        Instance = this as T;
    }
}

using Fusion;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : class
{
    public static T Instance;
    
    // Thêm chữ "virtual" để lỡ các class con (GameManager) muốn dùng Awake thì có thể Override
    protected virtual void Awake()
    {
        if(Instance != null && Instance != this as T)
        {
            Destroy(this.gameObject);
            return; 
        }
        Instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this as T)
        {
            Instance = null;
        }
    }
}
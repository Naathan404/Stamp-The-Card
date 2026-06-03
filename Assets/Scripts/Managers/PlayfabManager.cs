using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager Instance;

    public static event Action OnDataChanged;           // Thong bao da thay doi data de cap nhat lai UI

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    //Cap nhat display name
    public void UpdateDisplayName(string name, Action onSuccess, Action onError)
    {
        var request = new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = name
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => 
            {
                Debug.Log("Cap nhat Display name thanh cong!");
                LocalPlayerData.DisplayName = result.DisplayName;
                OnDataChanged?.Invoke();
                onSuccess?.Invoke();
            },
            error => 
            {
                if (error.Error == PlayFabErrorCode.NameNotAvailable)
                {
                    onError?.Invoke();
                }
                else
                {
                    onError?.Invoke();
                }

                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
    
    //Cap nhat thong so tran dau
    public void UpdateStatistics(int totalWins, int totalLoses, int rankPoints)
    {
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "TotalWins", Value = totalWins},
                new StatisticUpdate { StatisticName = "TotalLoses", Value = totalLoses},
                new StatisticUpdate { StatisticName = "RankPoints", Value = rankPoints }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result =>
            {
                Debug.Log("Cap nhat statistics thanh cong!");
                OnDataChanged?.Invoke();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    //Cong va tru soul
    public void AddSoul(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "SL",
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request,
            result => {
                LocalPlayerData.Souls = result.Balance;
                Debug.Log("Da cong souls. So du moi: " + result.Balance);
                OnDataChanged?.Invoke();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
    public void SubtractSoul(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "SL",
            Amount = amount
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result =>
            {
                LocalPlayerData.Souls = result.Balance;
                Debug.Log("Da tru souls. So du moi: " + result.Balance);
                OnDataChanged?.Invoke();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    //Cap phat stamp co ban cho new player
    public void GrantStamp()
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "grantStamps",
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                Debug.Log("Cap phat stamp co ban thanh cong!");
                PlayFabInventoryManager.Instance.GetPlayerInventory();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // Ham goi khi player thuc hien "Giao keo thuong"
    public void BuyNormalPack(Action<List<ItemInstance>> onSuccess)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_normal_pack",
            Price = 49,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            result =>
            {
                Debug.Log("Thuc hien giao keo thanh cong!");
                LocalPlayerData.Souls -= 49;

                PlayFabInventoryManager.Instance.GetPlayerInventory();

                onSuccess?.Invoke(result.Items);
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Normal_BundleInsufficientSoulsPlayGachaAnimation();
                }
                else
                    Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    // Ham goi khi player thuc hien "Giao keo vua"
    public void BuyMediumPack(Action<List<ItemInstance>> onSuccess)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_medium_pack",
            Price = 79,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            result =>
            {
                Debug.Log("Thuc hien giao keo thanh cong!");
                LocalPlayerData.Souls -= 79;

                PlayFabInventoryManager.Instance.GetPlayerInventory();

                onSuccess?.Invoke(result.Items);
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Medium_BundleInsufficientSoulsPlayGachaAnimation();
                }
                else
                    Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    // Ham goi khi player thuc hien "Giao keo to"
    public void BuyLargePack(Action<List<ItemInstance>> onSuccess)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_large_pack",
            Price = 129,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            result =>
            {
                Debug.Log("Thuc hien giao keo thanh cong!");
                LocalPlayerData.Souls -= 129;

                PlayFabInventoryManager.Instance.GetPlayerInventory();

                onSuccess?.Invoke(result.Items);
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Large_BundleInsufficientSoulsPlayGachaAnimation();
                }
                else
                    Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
}

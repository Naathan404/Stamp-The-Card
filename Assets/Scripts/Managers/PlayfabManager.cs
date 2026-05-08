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
    public void UpdateDisplayName(string name)
    {
        var request = new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = name
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => {
                Debug.Log("Cap nhat Display name thanh cong!");
                LocalPlayerData.DisplayName = result.DisplayName;
                OnDataChanged?.Invoke();
                },
            error => Debug.LogError(error.GenerateErrorReport())
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
}

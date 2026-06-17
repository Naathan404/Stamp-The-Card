using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;

public class PlayfabLeaderboardManager : MonoBehaviour
{
    public static event Action onLeaderBoardChanged;
    public static PlayfabLeaderboardManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "RankPoints",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLoadLeaderboardSuccess, OnLoadLeaderboardFailed);
    }

    private void OnLoadLeaderboardSuccess(GetLeaderboardResult result)
    {
        Debug.Log("Load leaderboard thanh cong!");

        //Don dep thong tin leaderboard tai local
        LocalPlayerData.leaderboardDatas.Clear();

        //Luu leaderboard vao local
        foreach (PlayerLeaderboardEntry player in result.Leaderboard)
        { 
            LeaderboardData data = new LeaderboardData();
            data.rank = player.Position + 1;
            data.name = player.DisplayName;
            data.rankPoints = player.StatValue;

            LocalPlayerData.leaderboardDatas.Add(data);
        }

        onLeaderBoardChanged?.Invoke();
    }

    private void OnLoadLeaderboardFailed(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}

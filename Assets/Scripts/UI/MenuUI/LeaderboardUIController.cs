using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;


public class LeaderboardUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _leaderboardRowPrefab;
    [SerializeField] private Transform _leaderboard;

    private void Awake()
    {
        PlayfabLeaderboardManager.Instance.LoadLeaderboard();
    }

    public void BackToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }

    public void UpdateLeaderboardUI()
    {
        // Don dep leaderboard
        for (int i = _leaderboard.childCount - 1; i >= 0; i--)
        {
            Destroy(_leaderboard.GetChild(i).gameObject);
        }

        // Load leaderboard de hien thi UI
        foreach (LeaderboardData data in LocalPlayerData.leaderboardDatas)
        {
            GameObject newSlot = Instantiate(_leaderboardRowPrefab, _leaderboard);

            LeaderboardRowUI rowUI = newSlot.GetComponent<LeaderboardRowUI>();
            if (rowUI != null)
            {
                rowUI.SetUpLeaderboardRowUI(data.rank, data.name, data.rankPoints);
            }
        }
    }

    private void OnEnable()
    {
        PlayfabLeaderboardManager.onLeaderBoardChanged += UpdateLeaderboardUI;
    }

    private void OnDisable()
    {
        PlayfabLeaderboardManager.onLeaderBoardChanged -= UpdateLeaderboardUI;
    }
}

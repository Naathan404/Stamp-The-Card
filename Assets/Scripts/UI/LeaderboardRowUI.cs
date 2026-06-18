using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _rankPointsText;

    public void SetUpLeaderboardRowUI(int rank, string playerName, int rankPoints)
    { 
        _rankText.text = rank.ToString();
        _playerNameText.text = playerName;
        _rankPointsText.text = rankPoints.ToString();
    }
}

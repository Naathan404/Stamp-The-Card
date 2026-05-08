using TMPro;
using UnityEngine;

public class AccountUIController : MonoBehaviour
{
    [Header("Acount Info UI")]
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _displaynameText;
    [SerializeField] private TextMeshProUGUI _soulAmountText;
    [SerializeField] private TextMeshProUGUI _rankPointsText;
    [SerializeField] private TextMeshProUGUI _totalWinsText;
    [SerializeField] private TextMeshProUGUI _totalLosesText;

    private void Start()
    {
        UpdateAccountUI();   
    }

    public void UpdateAccountUI()
    {
        _usernameText.text += LocalPlayerData.Username;
        _displaynameText.text += LocalPlayerData.DisplayName;
        _soulAmountText.text += LocalPlayerData.Souls;
        _rankPointsText.text += LocalPlayerData.RankPoints;
        _totalWinsText.text += LocalPlayerData.TotalWins;
        _totalLosesText.text += LocalPlayerData.TotalLoses;
    }

    public void BackToMenu()
    {
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
    }

    public void LogOut()
    {
        LocalPlayerData.Clear();

        SceneTransitionManager.Instance.LoadSceneAsync("Login");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
    }

    //Dang ky su kien khi data thay doi thi cap nhat lai UI
    private void OnEnable()
    {
        PlayfabManager.OnDataChanged += UpdateAccountUI;
    }
    private void OnDisable()
    {
        PlayfabManager.OnDataChanged -= UpdateAccountUI;
    }
}

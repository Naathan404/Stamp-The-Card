using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;


public class AccountUIController : MonoBehaviour
{
    [Header("Account Info UI")]
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _displaynameText;
    [SerializeField] private TextMeshProUGUI _soulAmountText;
    [SerializeField] private TextMeshProUGUI _rankPointsText;
    [SerializeField] private TextMeshProUGUI _totalWinsText;
    [SerializeField] private TextMeshProUGUI _totalLosesText;

    [Header("Edit Panel")]
    [SerializeField] private GameObject _editPanel;
    [SerializeField] private TMP_InputField _displaynameTextbox;


    private void Start()
    {
        if (_editPanel != null) _editPanel.SetActive(false);
        UpdateAccountUI();
    }

    public void UpdateAccountUI()
    {
        _usernameText.text = $"{LocalPlayerData.Username}";
        _displaynameText.text = $"{LocalPlayerData.DisplayName}";
        _soulAmountText.text = $"{LocalPlayerData.Souls}";
        _rankPointsText.text = $"{LocalPlayerData.RankPoints}";
        _totalWinsText.text = $"{LocalPlayerData.TotalWins}";
        _totalLosesText.text = $"{LocalPlayerData.TotalLoses}";
    }

    public void BackToMenu()
    {
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
    }

    public void EditName()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        _editPanel.SetActive(true);
        _displaynameTextbox.text = LocalPlayerData.DisplayName;
    }

    public void ChangePassword()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);

        
        // Gọi API đã có tích hợp Verify ngầm bên trong PlayfabManager
        PlayfabManager.Instance.ChangePassword(
            () => {
                Debug.Log("Gửi mail đổi mật khẩu thành công!");
            },
            () => {
                Debug.LogError("Gửi mail đổi mật khẩu thất bại");
            }
        );
    }

    public void LogOut()
    {
        PlayfabManager.Instance.SaveSelectedStamps(() =>
        {
            LocalPlayerData.Clear();
            SceneTransitionManager.Instance.LoadSceneAsync("Login");
            AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        });
    }

    // ==================== EDIT PANEL LOGIC ====================
    public void OkEditButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);

        string newDisplayName = _displaynameTextbox.text.Trim();


        if (string.IsNullOrEmpty(newDisplayName))
        {
            Debug.LogError("Display Name không được để trống!");
            return;
        }
            // Chỉ đổi Display Name
            ProceedToUpdateDisplayName(newDisplayName);
        
    }

    private void ProceedToUpdateDisplayName(string newDisplayName)
    {
        // 1. Kiểm tra xem PlayfabManager có bị quên chưa đưa vào Scene không
        if (PlayfabManager.Instance == null)
        {
            Debug.LogError("LỖI CHÍ MẠNG: Không tìm thấy PlayfabManager trong Scene! Hãy chắc chắn đã kéo script PlayfabManager vào một GameObject.");
            return;
        }

        if (newDisplayName != LocalPlayerData.DisplayName)
        {
            PlayfabManager.Instance.UpdateDisplayName(
                newDisplayName,
                () => {
                    Debug.Log("Cập nhật thông tin tài khoản thành công!");
                    _editPanel.SetActive(false);
                },
                () => {
                    Debug.LogError("Cập nhật Display Name thất bại từ phía Server.");
                }
            );
        }
        else
        {
            _editPanel.SetActive(false);
        }
    }

    public void CancelEditButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        _editPanel.SetActive(false);
    }

    

   

    private void OnEnable()
    {
        PlayfabManager.OnDataChanged += UpdateAccountUI;
    }
    private void OnDisable()
    {
        PlayfabManager.OnDataChanged -= UpdateAccountUI;
    }
}

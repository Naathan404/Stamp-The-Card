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
    [SerializeField] private TMP_InputField _usernameTextbox;
    [SerializeField] private TMP_InputField _displaynameTextbox;
    [SerializeField] private TMP_InputField _confirmPasswordEditTextbox; // Thêm ô nhập mật khẩu hiện tại để xác thực khi đổi tên

    [Header("Change Password Panel")]
    [SerializeField] private GameObject _changePasswordPanel;
    [SerializeField] private TMP_InputField _currentPasswordTextbox;
    [SerializeField] private TMP_InputField _newPasswordTextbox;
    [SerializeField] private TMP_InputField _confirmNewPasswordTextbox;

    private void Start()
    {
        if (_editPanel != null) _editPanel.SetActive(false);
        if (_changePasswordPanel != null) _changePasswordPanel.SetActive(false);
        UpdateAccountUI();
    }

    public void UpdateAccountUI()
    {
        _usernameText.text = $"Username: {LocalPlayerData.Username}";
        _displaynameText.text = $"Display Name: {LocalPlayerData.DisplayName}";
        _soulAmountText.text = $"Souls: {LocalPlayerData.Souls}";
        _rankPointsText.text = $"Rank Points: {LocalPlayerData.RankPoints}";
        _totalWinsText.text = $"Wins: {LocalPlayerData.TotalWins}";
        _totalLosesText.text = $"Loses: {LocalPlayerData.TotalLoses}";
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
        _usernameTextbox.text = LocalPlayerData.Username;
        _displaynameTextbox.text = LocalPlayerData.DisplayName;
        _confirmPasswordEditTextbox.text = string.Empty; // Reset ô pass xác nhận
    }

    public void ChangePassword()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        _changePasswordPanel.SetActive(true);
        ClearPasswordInputs();
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

        string newUsername = _usernameTextbox.text.Trim();
        string newDisplayName = _displaynameTextbox.text.Trim();
        string inputPassword = _confirmPasswordEditTextbox.text; // Mật khẩu do user nhập để xác thực

        if (string.IsNullOrEmpty(newUsername) || string.IsNullOrEmpty(newDisplayName))
        {
            Debug.LogError("Username và Display Name không được để trống!");
            return;
        }

        // Tình huống 1: Có thay đổi Username -> Bắt buộc kiểm tra mật khẩu hiện tại trước
        if (newUsername != LocalPlayerData.Username)
        {
            if (string.IsNullOrEmpty(inputPassword))
            {
                Debug.LogError("Vui lòng nhập mật khẩu hiện tại để xác thực việc đổi Username!");
                return;
            }

            PlayfabManager.Instance.UpdateUsername(
                newUsername,
                inputPassword,
                () => {
                    // Đổi Username thành công -> Tiếp tục đổi Display Name
                    ProceedToUpdateDisplayName(newDisplayName);
                },
                () => {
                    Debug.LogError("Đổi Username thất bại! Mật khẩu không chính xác hoặc tên đăng nhập bị trùng.");
                }
            );
        }
        else
        {
            // Tình huống 2: Giữ nguyên Username, chỉ đổi Display Name (PlayFab cho phép đổi DisplayName tự do không cần mật khẩu)
            ProceedToUpdateDisplayName(newDisplayName);
        }
    }

    private void ProceedToUpdateDisplayName(string newDisplayName)
    {
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

    // ==================== CHANGE PASSWORD PANEL LOGIC ====================
    public void OkChangePassButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);

        string currentPassword = _currentPasswordTextbox.text;
        string newPassword = _newPasswordTextbox.text;
        string confirmPassword = _confirmNewPasswordTextbox.text;

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
        {
            Debug.LogError("Vui lòng điền đầy đủ các ô mật khẩu!");
            return;
        }

        if (newPassword != confirmPassword)
        {
            Debug.LogError("Mật khẩu mới và Xác nhận mật khẩu không khớp!");
            return;
        }

        if (newPassword.Length < 6)
        {
            Debug.LogError("Mật khẩu mới phải có ít nhất 6 ký tự!");
            return;
        }

        // Gọi API đã có tích hợp Verify ngầm bên trong PlayfabManager
        PlayfabManager.Instance.ChangePassword(
            currentPassword,
            newPassword,
            () => {
                Debug.Log("Đổi mật khẩu thành công!");
                _changePasswordPanel.SetActive(false);
                ClearPasswordInputs();
            },
            () => {
                Debug.LogError("Đổi mật khẩu thất bại. Mật khẩu hiện tại nhập vào không chính xác!");
            }
        );
    }

    public void CancelChangePassButton()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        _changePasswordPanel.SetActive(false);
        ClearPasswordInputs();
    }

    private void ClearPasswordInputs()
    {
        _currentPasswordTextbox.text = string.Empty;
        _newPasswordTextbox.text = string.Empty;
        _confirmNewPasswordTextbox.text = string.Empty;
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

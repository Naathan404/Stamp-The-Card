using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("Login Panel Elements")]
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _signUpButton;
    [SerializeField] private Button _googleButton;
    [SerializeField] private Button _facebookButton;
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_InputField _passwordInput;

    [Header("Notification Panel Elements")]
    [SerializeField] private GameObject _notificationPanel;
    [SerializeField] private TMP_Text _notificationText;
    private Vector2 _notificationOriginalScale;
    [SerializeField] private float _notificationDuration = 2f;
    [SerializeField] private float _popUpDuration = 0.5f;

    [Header("Validation Settings")]
    [SerializeField] private int _maxUsernameLength = 12;
    [SerializeField] private int _minPasswordLength = 6;
    private string _currentUsername = string.Empty;
    private string _currentPassword = string.Empty;
    private Vector2 _originalUsernameInputPosition;
    private Vector2 _originalPasswordInputPosition;

    [Header("Displayname Input Panel")]
    [SerializeField] private GameObject _displayNameInputPanel;
    [SerializeField] private TMP_InputField _displayNameInput;
    [SerializeField] private Button _confirmButton;

    private void Awake()
    {
        _loginButton.onClick.AddListener(OnLoginButtonClicked);
        _signUpButton.onClick.AddListener(OnSignUpButtonClicked);
        _googleButton.onClick.AddListener(OnGoogleButtonClicked);
        _facebookButton.onClick.AddListener(OnFacebookButtonClicked);
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);

        _originalUsernameInputPosition = _usernameInput.transform.position;
        _originalPasswordInputPosition = _passwordInput.transform.position;

        _notificationOriginalScale = _notificationPanel.transform.localScale;
        _notificationPanel.SetActive(false);
    }

    private void OnEnable()
    {
        _usernameInput.text = string.Empty;
        _passwordInput.text = string.Empty;

        _usernameInput.onValueChanged.AddListener(OnUsernameInputChanged);
        _passwordInput.onValueChanged.AddListener(OnPasswordInputChanged);
    }

    private void OnDisable()
    {
        _usernameInput.onValueChanged.RemoveListener(OnUsernameInputChanged);
        _passwordInput.onValueChanged.RemoveListener(OnPasswordInputChanged);
    }



    private void OnDestroy()
    {
        _loginButton.onClick.RemoveListener(OnLoginButtonClicked);
        _signUpButton.onClick.RemoveListener(OnSignUpButtonClicked);
        _googleButton.onClick.RemoveListener(OnGoogleButtonClicked);
        _facebookButton.onClick.RemoveListener(OnFacebookButtonClicked);
        _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
    }

    public void DisplayInputDisplaynamePanel()
    { 
        _displayNameInputPanel.SetActive(true);
    }

    public void OnConfirmButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);

        string displayName = _displayNameInput.text;
        if (string.IsNullOrEmpty(displayName))
        {
            Debug.Log("Vui long khong de trong display name!");
            return;
        }

        _displayNameInputPanel.SetActive(false);
        PlayfabManager.Instance.UpdateDisplayName(displayName);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }    

    public void OnLoginButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);

        string username = _usernameInput.text;
        string password = _passwordInput.text;

        // Validate input
        if (!ValidateInput(username, password))
        {
            return;
        }

        // call login method
        AuthManager.Instance.Login(username, password);
    }
    
    public void OnSignUpButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);

        string username = _usernameInput.text;
        string password = _passwordInput.text;

        // validate input
        if (!ValidateInput(username, password))
        {
            return;
        }

        // call sign up method
        AuthManager.Instance.SignUp(username, password);
    }

    public void OnGoogleButtonClicked()
    {
        AuthManager.Instance.LoginWithGoogle();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
    }

    public void OnFacebookButtonClicked()
    {
        AuthManager.Instance.LoginWithFacebook();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
    }


    /// <summary>
    /// Validate username and password input
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    private bool ValidateInput(string username, string password)
    {
        if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Username or password is empty!");
            if(string.IsNullOrEmpty(username))
            {
                _usernameInput.placeholder.GetComponent<TMP_Text>().text = "Username is required";
                _usernameInput.placeholder.GetComponent<TMP_Text>().color = Color.red;
                _usernameInput.placeholder.GetComponent<TMP_Text>().alpha = 0.7f;
            }
            if(string.IsNullOrEmpty(password))
            {
                _passwordInput.placeholder.GetComponent<TMP_Text>().text = "Password is required";
                _passwordInput.placeholder.GetComponent<TMP_Text>().color = Color.red;
                _passwordInput.placeholder.GetComponent<TMP_Text>().alpha = 0.7f;
            }
            return false;
        }

        if(password.Length < _minPasswordLength)
        {
            Debug.LogWarning($"Password must be at least {_minPasswordLength} characters!");
            _passwordInput.text = string.Empty; // Clear the password field
            _passwordInput.placeholder.GetComponent<TMP_Text>().text = $"Password must be at least {_minPasswordLength} characters";
            _passwordInput.placeholder.GetComponent<TMP_Text>().color = Color.red;
            _passwordInput.placeholder.GetComponent<TMP_Text>().alpha = 0.7f;
            return false;
        }
        return true;
    }

    private void OnUsernameInputChanged(string newValue)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.InputFieldClick, true);
        if (newValue.Length > _maxUsernameLength)
        {
            _usernameInput.transform.DOShakePosition(0.5f, new Vector3(10f, 0, 0), 20, 90).OnComplete(() =>
            {
                _usernameInput.transform.position = _originalUsernameInputPosition;
            });
            _usernameInput.text = _currentUsername; // Revert to the last valid username
            return;
        }

        if(newValue.Contains(" "))
        {
            _usernameInput.transform.DOShakePosition(0.5f, new Vector3(10f, 0, 0), 20, 90).OnComplete(() =>
            {
                _usernameInput.transform.position = _originalUsernameInputPosition;
            });
            _usernameInput.text = _currentUsername; // Revert to the last valid username
            return;
        }
        _currentUsername = _usernameInput.text;
    }

    private void OnPasswordInputChanged(string newValue)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.InputFieldClick, true);
        if(newValue.Length > _maxUsernameLength)
        {
            _passwordInput.transform.DOShakePosition(0.5f, new Vector3(10f, 0, 0), 20, 90).OnComplete(() =>
            {
                _passwordInput.transform.position = _originalPasswordInputPosition;
            });
            _passwordInput.text = _currentPassword; // Revert to the last valid password
            return;
        }
        
        if(_passwordInput.text.Contains(" "))
        {
            _passwordInput.transform.DOShakePosition(0.5f, new Vector3(10f, 0, 0), 20, 90).OnComplete(() =>
            {
                _passwordInput.transform.position = _originalPasswordInputPosition;
            });
            _passwordInput.text = _currentPassword; // Revert to the last valid password
            return;
        }
        _currentPassword = _passwordInput.text;
    }

public void ShowNotification(string message)
    {
        _notificationPanel.transform.DOKill(); 
        StopAllCoroutines(); 

        _notificationPanel.SetActive(true);
        _notificationPanel.transform.localScale = Vector2.zero; 
        StartCoroutine(HandleNotification(message, _notificationDuration));
    }

    private IEnumerator HandleNotification(string message, float duration)
    {
        _notificationText.text = message;
        _notificationPanel.transform.DOScale(_notificationOriginalScale, _popUpDuration).SetEase(Ease.OutBack);
        
        yield return new WaitForSeconds(_popUpDuration);
        yield return new WaitForSeconds(duration);

        _notificationPanel.transform.DOScale(Vector2.zero, _popUpDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            _notificationPanel.SetActive(false);
        }); 
    }

    public string GetCurrentUsername()
    {
        return _currentUsername;
    }
}

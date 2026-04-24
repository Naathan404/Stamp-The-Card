using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class AuthManager : Singleton<AuthManager>
{
    private LoginUI _loginUI;
    private void Start()
    {
        if(_loginUI == null)
            _loginUI = FindAnyObjectByType<LoginUI>();
    }

    // Login with PlayFab
    public void Login(string username, string password)
    {
        Debug.Log("Attempting to log in with username: " + username);

        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        // call api to login
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
    }

    // Sign up with PlayFab 
    public void SignUp(string username, string password)
    {
        Debug.Log("Attempting to sign up with username: " + username); 

        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        // call api to sign up
        PlayFabClientAPI.RegisterPlayFabUser(request, OnSignUpSuccess, OnError);
    }

    // Login with Google
    public void LoginWithGoogle()
    {
        Debug.Log("This feature is currently unavailable.");

        // var request = new LoginWithGoogleAccountRequest
        // {

        //     CreateAccount = true,
        // };

        // // api call to login with google
        // PlayFabClientAPI.LoginWithGoogleAccount(request, OnLoginSuccess, OnError);
    }

    // Login with Facebook
    public void LoginWithFacebook()
    {
        Debug.Log("This feature is currently unavailable.");
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("<color=green>Login API call successful! Welcome, " + result.PlayFabId + "</color>");
        
        PlayerPrefs.SetString("PlayFabId", result.PlayFabId);
        PlayerPrefs.SetString("PlayerName", _loginUI.GetCurrentUsername());
        // load main menu scene
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }    

    private void OnSignUpSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("<color=green>Sign up API call successful! Welcome, " + result.PlayFabId + "</color>");
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log("API call failed!");

        if (error.Error == PlayFabErrorCode.AccountNotFound)
            Debug.LogAssertion("Tài khoản không tồn tại!");
        else if (error.Error == PlayFabErrorCode.InvalidUsernameOrPassword)
            Debug.LogAssertion("Sai mật khẩu!");
        else if (error.Error == PlayFabErrorCode.UsernameNotAvailable)
            Debug.LogAssertion("Tên tài khoản này đã có người xài!");
        else
            Debug.LogError("Lỗi kết nối: " + error.ErrorMessage);

        Debug.LogError(error.GenerateErrorReport());
    }

}

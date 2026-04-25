using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class MatchFindUI : MonoBehaviour
{
    [Header("LAUNCHER")]
    [SerializeField] private Launcher _launcher;
    [Header("UI ELEMENTS")]
    [SerializeField] private GameObject _waitingPanel;
    [SerializeField] private TMP_InputField _roomNameField;
    [SerializeField] private List<Button> _uiButtonsToUnactive; 

    [Header("GENERAL ELEMENTS")]
    [SerializeField] private TMP_InputField _usernameTMP;

    public static MatchFindUI Instance;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Buttons
    public void OnQuickPlayButtonClicked()
    {
        // vô hiệu các nút
        foreach(var btn in _uiButtonsToUnactive)
        {
            btn.interactable = false;
        }

        // bật panel waiting...
        _waitingPanel.SetActive(true); 

        string currentName = _usernameTMP.text.IsNullOrEmpty() ? "Player" : _usernameTMP.text;
        LocalPlayerData.Username = currentName;
        _launcher.StartGame(GameMode.AutoHostOrClient, string.Empty);
    }

    public void OnCustomRoomButtonClicked()
    {
        foreach(var btn in _uiButtonsToUnactive)
        {
            btn.interactable = false;
        }
        _waitingPanel.SetActive(true); 
        
        string roomName = _roomNameField.text;
        if(string.IsNullOrEmpty(roomName)) roomName = "QuickMatchRoom";
        string currentName = _usernameTMP.text.IsNullOrEmpty() ? "Player" : _usernameTMP.text;
        LocalPlayerData.Username = currentName;
        _launcher.StartGame(GameMode.AutoHostOrClient, roomName); 
    }     

    public void LoadScene(string sceneName)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync(sceneName);
    }
}

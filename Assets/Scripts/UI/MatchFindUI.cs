using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchFindUI : MonoBehaviour
{
    [Header("LAUNCHER")]
    [SerializeField] private Launcher _launcher;
    [Header("UI ELEMENTS")]
    [SerializeField] private TMP_InputField _roomNameField;
    [SerializeField] private List<Button> _uiButtonsToUnactive; 

    // Buttons
    public void OnQuickPlayButtonClicked()
    {
        foreach(var btn in _uiButtonsToUnactive)
        {
            btn.interactable = false;
        }
        _launcher.StartGame(GameMode.AutoHostOrClient, string.Empty);
    }

    public void OnCustomRoomButtonClicked()
    {
        foreach(var btn in _uiButtonsToUnactive)
        {
            btn.interactable = false;
        }
        string roomName = _roomNameField.text;
        if(string.IsNullOrEmpty(roomName)) roomName = "Default_Room";
        _launcher.StartGame(GameMode.AutoHostOrClient, roomName); 
    }     
}

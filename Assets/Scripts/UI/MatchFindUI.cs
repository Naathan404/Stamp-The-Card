using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
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
    [SerializeField] private TextMeshProUGUI _findFoeText;

    private List<string> _findFoeLines = new List<string>();
    private float _timeToChangeFindFoeText = 2.0f;
    private Sequence _textSequence;

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

    private void Start()
    {
        _findFoeLines.Add("Seeking a Victim...");
        _findFoeLines.Add("Awaiting the Next Fate...");
        _findFoeLines.Add("Shuffling Deck...");
        _findFoeLines.Add("Preparing the Board...");
        _findFoeLines.Add("Awaiting Challenger...");

        _findFoeText.text = _findFoeLines[0];
    }

    private void StartMatchmakingAnimation()
    {
        _findFoeText.DOKill();
        _findFoeText.transform.DOKill();

        if(_textSequence != null) _textSequence.Kill();

        _findFoeText.alpha = 0f;
        _findFoeText.transform.localScale = Vector2.one;
        ChangeMatchmakingText();

        _findFoeText.transform.DOScale(Vector3.one * 1.05f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        _textSequence = DOTween.Sequence();
        _textSequence.Append(_findFoeText.DOFade(1f, 0.5f));
        _textSequence.AppendInterval(1.5f);
        _textSequence.Append(_findFoeText.DOFade(0f, 0.5f));
        _textSequence.AppendCallback(ChangeMatchmakingText);
        _textSequence.SetLoops(-1);
    }

    private void ChangeMatchmakingText()
    {
        _findFoeText.text = _findFoeLines[UnityEngine.Random.Range(0, _findFoeLines.Count)];
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
        StartMatchmakingAnimation();

        string currentName = _usernameTMP.text.IsNullOrEmpty() ? "Player" : _usernameTMP.text;
        LocalPlayerData.Username = currentName;
        _launcher.FindQuickMatch();
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
        _launcher.CreateCustomRoom(GameMode.AutoHostOrClient, roomName); 
    }     

    public void OnCancelMatchmakingButtonClicked()
    {
        _findFoeText.DOKill();
        _findFoeText.transform.DOKill();
        if (_textSequence != null) _textSequence.Kill();

        _waitingPanel.SetActive(false);
        foreach(var btn in _uiButtonsToUnactive)
        {
            btn.interactable = true;
        }

        _launcher.CancelMatchmaking();
    }

    public void LoadScene(string sceneName)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync(sceneName);
    }                                                            
}
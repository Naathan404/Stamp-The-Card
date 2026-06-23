using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _bottomHpText;
    [SerializeField] private TextMeshProUGUI _topHpText;
    [SerializeField] private TextMeshProUGUI _bottomNameText;
    [SerializeField] private TextMeshProUGUI _topNameText;
    [SerializeField] private TextMeshPro _bottomStampCount;
    [SerializeField] private TextMeshPro _topStampCount;

    [Header("GameOver UI")]
    public CanvasGroup GameOverPanel;
    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI MessageText;
    public RectTransform MenuButton;
    public TextMeshProUGUI SoulText;
    public TextMeshProUGUI RankPointText;
    private List<string> _loseMessages = new List<string>();
    private List<string> _winMessages = new List<string>();
    private float _gameOverAppearTime = 1.0f;

    [Header("Seat Transforms")]
    [SerializeField] private Transform _bottomSeatTransform;
    [SerializeField] private Transform _topSeatTransform;

    [Header("Juice UI")]
    public CanvasGroup BlackScreenCurtain;
    public CanvasGroup BattleStartPanelGroup; 
    public RectTransform BattleStartTextRect;
    public TextMeshProUGUI BattleStartTMP;
    public RectTransform TurnAnnouncementRect; 
    public TextMeshProUGUI TurnAnnouncementTMP; 
    public CanvasGroup TurnAnnouncementCanvasGroup;

    [Header("Juice Time Variable")]
    [SerializeField] private float _matchFoundTextAppearDuration = 0.5f;
    [SerializeField] private float _matchFoundTextDuration = 1.5f;
    [SerializeField] private float _turnTextAppearDuration = 0.5f;
    [SerializeField] private float _turnTextDuration = 1f;
    [SerializeField] private float _battleStartTextAppearDuration = 1f;
    [SerializeField] private float _battleStartTextDuration = 1f;

    [Header("End Phase Cinematic UI")]
    public TextMeshProUGUI PlayerCenterText;
    public TextMeshProUGUI OppCenterText;
    public TextMeshProUGUI FinalDamageText;
    public Transform PlayerAvatarTransform; // Vị trí bay vào nếu mình thua
    public Transform OppAvatarTransform;    // Vị trí bay vào nếu địch thua
    [SerializeField] private float _mergeTime = 0.5f;
    [SerializeField] private float _countTime = 1.0f;
    [SerializeField] private float _attackTime = 0.8f;
    [SerializeField] private float _flashTime = 0.4f;

    private List<string> _drawLines = new List<string>();

    private Vector3 _originalPlayerCenterTextPosition;
    private Vector3 _originalOppCenterTextPosition;
    private Vector3 _originalFinalDamageTextPosition;

    Color grayColor = new Color(0.6f, 0.6f, 0.6f, 1f);



    public static UIManager Instance;
    private void Awake()
    {
        if(Instance != null  && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        DOTween.defaultTimeScaleIndependent = true;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        _bottomHpText.text = "15";
        _topHpText.text = "15";
        _bottomStampCount.text = "9";
        _topStampCount.text = "9";
        BlackScreenCurtain.gameObject.SetActive(true);
        BlackScreenCurtain.alpha = 1f;

        _originalPlayerCenterTextPosition = PlayerCenterText.transform.position;
        _originalOppCenterTextPosition = OppCenterText.transform.position;
        _originalFinalDamageTextPosition = FinalDamageText.transform.position;

        GameOverPanel.gameObject.SetActive(false);

        _drawLines.Add("SAFE... FOR NOW");
        _drawLines.Add("NO ONE DIES THIS TURN");
        _drawLines.Add("STILL BREATHING");
        _drawLines.Add("DEATH IS DELAYED");
        _drawLines.Add("A HOLLOW DRAW");
        _drawLines.Add("NO VICTORY, NO VICTIM");

        _winMessages.Add("This soul belongs to me!");
        _winMessages.Add("Thanks for the soul!");
        _winMessages.Add("Another soul sealed!");

        _loseMessages.Add("You have forfeited your right to exist!");
        _loseMessages.Add("The price of your gamble is your soul!");
        _loseMessages.Add("Paid for with your life!");
        _loseMessages.Add("The defeated have no voice");
    }

    public void UpdateHpTexts(bool amIHost)
    {
        int oldBottomHp = 0, oldTopHp = 0;
        int.TryParse(_bottomHpText.text.Replace("HP: ", ""), out oldBottomHp);
        int.TryParse(_topHpText.text.Replace("HP: ", ""), out oldTopHp);

        int newBottomHp = amIHost ? GameManager.Instance.HostHP : GameManager.Instance.ClientHP;
        int newTopHp = amIHost ? GameManager.Instance.ClientHP : GameManager.Instance.HostHP;

        AnimateHPText(_bottomHpText, oldBottomHp, newBottomHp, true);
        AnimateHPText(_topHpText, oldTopHp, newTopHp, false);
    }

    /// <summary>
    /// Annimationn Modify Máu của 2 người chơi
    /// </summary>
    /// <param name="hpText"></param>
    /// <param name="oldHp"></param>
    /// <param name="newHp"></param>
    /// <param name="isMyHp"></param>
    private void AnimateHPText(TextMeshProUGUI hpText, int oldHp, int newHp, bool isMyHp)
    {
        if (oldHp == newHp || oldHp == 0) 
        {
            hpText.text = $"HP: {newHp}";
            return;
        }

        hpText.transform.DOKill(true);

        if (newHp < oldHp) // NẾU CÓ SÁT THƯƠNG
        {
            if (isMyHp) 
            {
                // MÌNH BỊ ĐÁNH
                FilterManager.Instance.FlashScreen(FilterManager.Instance.HazardColor);
                hpText.color = Color.red;
                hpText.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, vibrato: 10, elasticity: 1);
                hpText.transform.DOShakePosition(0.5f, strength: 15f);
                
                // Rung cả Camera để thấy chấn động
                if (Camera.main != null) 
                {
                    Camera.main.transform.DOComplete(); 
                    Camera.main.transform.DOShakePosition(0.4f, strength: 0.3f, vibrato: 20);
                }
            }
            else
            {
                // ĐỊCH BỊ ĐÁNH 
                // Đổi màu cam rực (Critical)
                FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor);
                hpText.color = FilterManager.Instance.HazardColor; 
                // Phình to hơn bình thường nhưng không rung Camera
                hpText.transform.DOPunchScale(Vector3.one * 0.8f, 0.4f, vibrato: 15);
                // Lắc ngả nghiêng Text thay vì lắc vị trí 
                hpText.transform.DOShakeRotation(0.4f, strength: new Vector3(0, 0, 20f), vibrato: 15);

                // Rung cả Camera để thấy chấn động
                if (Camera.main != null) 
                {
                    Camera.main.transform.DOComplete(); 
                    Camera.main.transform.DOShakePosition(0.4f, strength: 0.2f, vibrato: 15);
                }
            }

            // Trả về màu trắng sau khi giật xong
            hpText.DOColor(Color.white, 0.5f).SetDelay(0.5f);
        }
        else // NẾU ĐƯỢC HỒI MÁU
        {
            FilterManager.Instance.FlashScreen(FilterManager.Instance.AdvantageColor);
            hpText.color = Color.green;
            hpText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, vibrato: 5);
            hpText.DOColor(Color.white, 0.5f).SetDelay(0.5f);
        }

        DOTween.To(() => oldHp, x => {
            hpText.text = $"HP: {x}";
        }, newHp, 0.6f).SetEase(Ease.OutQuad);
    }

    public void UpdateStampCount(bool amIhost)
    {
        if(amIhost)
        {
            _bottomStampCount.text = GameManager.Instance.NetworkedHostStampCount.ToString();
            _topStampCount.text = GameManager.Instance.NetworkedClientStampCount.ToString();
        }
        else
        {
            _bottomStampCount.text = GameManager.Instance.NetworkedClientStampCount.ToString(); 
            _topStampCount.text = GameManager.Instance.NetworkedHostStampCount.ToString();
        }
    }

    public void SetSeatPosition(PlayerNetworkData playerData, bool isYourself)
    {
        if(isYourself)
        {
            playerData.gameObject.transform.position = _bottomSeatTransform.position;
            playerData.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.deepSkyBlue;
        }
        else
        {
            playerData.gameObject.transform.position = _topSeatTransform.position;
            playerData.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.softRed;

        }
    }

    public void UpdateNameUI(PlayerNetworkData playerData, bool isYourSelf)
    {
        Debug.Log("[UIMANAGER] Cập nhật tên hiển thị");
        // cập nhật tên cho bản thân
        if(isYourSelf)
            _bottomNameText.text = playerData.DisplayName.ToString();
        else    // cập nhật tên cho đối thủ
            _topNameText.text = playerData.DisplayName.ToString();
    }

    // public void ShowBattleStartAnnouncement()
    // {
    //     if (BattleStartPanelGroup == null || BattleStartTextRect == null) return;

    //     BattleStartPanelGroup.DOKill();
    //     BattleStartTextRect.DOKill();
        
    //     BattleStartPanelGroup.gameObject.SetActive(true);
        
    //     BattleStartPanelGroup.alpha = 0f;
        
    //     BattleStartTextRect.localScale = Vector3.one;
    //     BattleStartTextRect.anchoredPosition = new Vector2(-1500f, 0f);

    //     if (GameManager.Instance.Runner.IsServer)
    //     {
    //         if (GameStateManager.Instance.CurrentTurn % 2 == 1)
    //             BattleStartTMP.text = "YOU GO FIRST!";
    //         else
    //             BattleStartTMP.text = "YOU GO SECOND!";
    //     }
    //     else
    //     {
    //         if (GameStateManager.Instance.CurrentTurn % 2 == 1)
    //             BattleStartTMP.text = "YOU GO SECOND!";
    //         else
    //             BattleStartTMP.text = "YOU GO FIRST!";
    //     }


    //     Sequence seq = DOTween.Sequence();
    //     seq.AppendInterval(0.5f);
    //     seq.Append(BattleStartPanelGroup.DOFade(1f, 0.5f));
        
    //     seq.Join(BattleStartTextRect.DOAnchorPosX(0f, _battleStartTextAppearDuration).SetEase(Ease.OutBack));
    //     //seq.Join(BattleStartTextRect.DOPunchScale(Vector3.one * 0.5f, _battleStartTextAppearDuration, vibrato: 10, elasticity: 1));

    //     if (Camera.main != null)
    //     {
    //         seq.Join(Camera.main.transform.DOShakePosition(0.5f, strength: 0.2f, vibrato: 15));
    //     }

    //     seq.AppendInterval(_battleStartTextDuration);
    //     seq.Append(BattleStartTextRect.DOAnchorPosX(2000f, _battleStartTextAppearDuration).SetEase(Ease.InBack));
    //     seq.Join(BattleStartPanelGroup.DOFade(0f, _battleStartTextAppearDuration * 0.8f));

    //     seq.OnComplete(() => {
    //         BattleStartPanelGroup.gameObject.SetActive(false);
    //     });
    // }

    public void ShowBattleStartAnnouncement()
    {
        if (BattleStartPanelGroup == null || BattleStartTextRect == null) return;

        BattleStartPanelGroup.DOKill();
        BattleStartTextRect.DOKill();
        
        BattleStartPanelGroup.gameObject.SetActive(true);
        BattleStartPanelGroup.alpha = 0f;

        bool isHost = GameManager.Instance.Runner.IsServer;
        bool isTurnOdd = GameStateManager.Instance.CurrentTurn % 2 == 1;
        bool iGoFirst = (isHost && isTurnOdd) || (!isHost && !isTurnOdd);

        BattleStartTMP.text = iGoFirst ? "YOU GO FIRST!" : "YOU GO SECOND!";

        BattleStartTextRect.anchoredPosition = new Vector2(-1500f, 0f);
        BattleStartTextRect.localScale = Vector3.one * 2f; 
        BattleStartTextRect.localRotation = Quaternion.Euler(0, 0, 15f); 

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.2f); 

        seq.Append(BattleStartPanelGroup.DOFade(1f, 0.3f));
        seq.Join(BattleStartTextRect.DOAnchorPosX(0f, _battleStartTextAppearDuration).SetEase(Ease.OutExpo));
        seq.Join(BattleStartTextRect.DOScale(Vector3.one, _battleStartTextAppearDuration).SetEase(Ease.OutBack, 2f));
        seq.Join(BattleStartTextRect.DORotate(Vector3.zero, _battleStartTextAppearDuration).SetEase(Ease.OutBack));

        seq.AppendCallback(() => {
            // if (Camera.main != null)
            // {
            //     Camera.main.transform.DOComplete();
            //     Camera.main.transform.DOShakePosition(0.4f, strength: 0.4f, vibrato: 20);
            // }
            if (FilterManager.Instance != null)
            {
                FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor, 0.2f);
            }
        });

        seq.Append(BattleStartTextRect.DOAnchorPosX(100f, _battleStartTextDuration).SetEase(Ease.Linear));

        seq.Append(BattleStartTextRect.DOAnchorPosX(2000f, _battleStartTextAppearDuration).SetEase(Ease.InBack, 1.5f));
        seq.Join(BattleStartTextRect.DOScale(Vector3.one * 0.5f, _battleStartTextAppearDuration).SetEase(Ease.InBack));
        seq.Join(BattleStartTextRect.DORotate(new Vector3(0, 0, -15f), _battleStartTextAppearDuration).SetEase(Ease.InBack));
        
        seq.Join(BattleStartPanelGroup.DOFade(0f, _battleStartTextAppearDuration));

        seq.OnComplete(() => {
            BattleStartPanelGroup.gameObject.SetActive(false);
        });
    }

    public void ShowTurnAnnouncement(int turnNumber)
    {
        if (TurnAnnouncementRect == null || TurnAnnouncementTMP == null || TurnAnnouncementCanvasGroup == null) return;

        TurnAnnouncementRect.DOKill();
        TurnAnnouncementCanvasGroup.DOKill();
        TurnAnnouncementRect.gameObject.SetActive(true);
        
        //TurnAnnouncementTMP.text = $"TURN {turnNumber}"; 
        if (BlackScreenCurtain != null)
        {
            BlackScreenCurtain.gameObject.SetActive(true);
            BlackScreenCurtain.alpha = 1f;
        }

        Sequence seq = DOTween.Sequence();

        if(turnNumber == 1)
        {
            TurnAnnouncementTMP.text = $"SOUL LINKED"; 
            TurnAnnouncementRect.localScale = Vector3.one * 1.5f;
            TurnAnnouncementCanvasGroup.alpha = 0f; 
             // canvas mờ hiện leennn và text xuất hiện

            seq.Append(TurnAnnouncementCanvasGroup.DOFade(1f, 0.5f));
            seq.Join(TurnAnnouncementRect.DOScale(Vector3.one, _matchFoundTextAppearDuration).SetEase(Ease.OutBack));
            seq.AppendInterval(_matchFoundTextDuration);
            seq.Append(TurnAnnouncementCanvasGroup.DOFade(0f, 0.5f));

            seq.AppendCallback(() =>
            {
               TurnAnnouncementTMP.text = $"DRAW {turnNumber}";
               TurnAnnouncementRect.localScale = Vector3.one * 3f; 
            });
        }
        else
        {
            TurnAnnouncementTMP.text = $"DRAW {turnNumber}";
            TurnAnnouncementRect.localScale = Vector3.one * 3f;
            TurnAnnouncementCanvasGroup.alpha = 0f;
        }

        // seq.Append(TurnAnnouncementRect.DOScale(Vector3.one, _turnTextAppearDuration).SetEase(Ease.OutExpo));
        // seq.Join(TurnAnnouncementCanvasGroup.DOFade(1f, 0.5f));
        seq.Append(TurnAnnouncementRect.DOScale(Vector3.one, _turnTextAppearDuration).SetEase(Ease.OutBack, overshoot: 1.5f));
        seq.Join(TurnAnnouncementCanvasGroup.DOFade(1f, 0.4f));

        seq.AppendCallback(() => {
            if (Camera.main != null) 
            {
                Camera.main.transform.DOComplete();
                Camera.main.transform.DOShakePosition(0.3f, strength: 0.15f, vibrato: 15);
            }
        });

        //seq.AppendInterval(_turnTextDuration);
        seq.Append(TurnAnnouncementRect.DOScale(Vector3.one * 1.08f, _turnTextDuration).SetEase(Ease.Linear));

        // seq.Append(TurnAnnouncementRect.DOScale(Vector3.one * 1.5f, _turnTextAppearDuration).SetEase(Ease.InQuad));
        // seq.Join(TurnAnnouncementCanvasGroup.DOFade(0f, 0.5f));
        seq.Append(TurnAnnouncementRect.DOScale(Vector3.one * 2f, _turnTextAppearDuration).SetEase(Ease.InExpo));
        seq.Join(TurnAnnouncementCanvasGroup.DOFade(0f, _turnTextAppearDuration).SetEase(Ease.InExpo));

        if (BlackScreenCurtain != null)
        {
            seq.AppendInterval(0.05f);
            seq.Join(BlackScreenCurtain.DOFade(0f, 0.4f).SetEase(Ease.InOutSine));
        }

        seq.OnComplete(() => {
            TurnAnnouncementRect.gameObject.SetActive(false);
            if (BlackScreenCurtain != null)
            {
                BlackScreenCurtain.gameObject.SetActive(false);
            }
        });
    }

    public IEnumerator CinematicEndPhaseRoutine(int hostRawScore, int clientRawScore)
    {
        bool amIHost = GameManager.Instance.Runner.IsServer; 
        int playerRawScore = amIHost ? hostRawScore : clientRawScore;
        int oppRawScore = amIHost ? clientRawScore : hostRawScore;

        // --- bước 0: Tính toán Logic
        int playerRealScore = playerRawScore % 10;
        int oppRealScore = oppRawScore % 10;
        int damage = Mathf.Abs(playerRealScore - oppRealScore);
        
        // Nếu điểm thực của mình nhỏ hơn địch -> Mình ăn đấm
        bool iTakeDamage = playerRealScore < oppRealScore; 

        // Tọa độ điểm chính giữa màn hình
        Vector3 centerScreenPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        // Setup khởi tạo
        PlayerCenterText.transform.localScale = Vector2.zero;
        OppCenterText.transform.localScale = Vector2.zero;
        OppCenterText.gameObject.SetActive(true);
        FinalDamageText.gameObject.SetActive(false);
        
        PlayerCenterText.text = "0";
        OppCenterText.text = "0";
        PlayerCenterText.transform.localScale = Vector3.one;
        OppCenterText.transform.localScale = Vector3.one;
        PlayerCenterText.transform.position = _originalPlayerCenterTextPosition;
        OppCenterText.transform.position = _originalOppCenterTextPosition;
        FinalDamageText.transform.position = _originalFinalDamageTextPosition;
        FinalDamageText.alpha = 1f;
        FinalDamageText.transform.localScale = Vector3.one;

        FilterManager.Instance.SetDramaticFilter(true);

        //  bước 1: ĐẾM ĐIỂM ĐỐI THỦ (1 giây) 
        OppCenterText.transform.DOScale(1f, 0.25f).WaitForCompletion();
        yield return DOTween.To(() => 0, x => OppCenterText.text = x.ToString(), oppRawScore, _countTime)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        yield return new WaitForSeconds(0.2f);

        PlayerCenterText.gameObject.SetActive(true);
        PlayerCenterText.transform.DOScale(1f, 0.25f).WaitForCompletion();
        // bước 2: ĐẾM ĐIỂM CỦA MÌNH (1 giây)
        yield return DOTween.To(() => 0, x => PlayerCenterText.text = x.ToString(), playerRawScore, _countTime)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        yield return new WaitForSeconds(0.5f);

        // -bbước 3: GIẢM ĐỒNG LOẠT VỀ SỐ DƯ
        DG.Tweening.Sequence modSeq = DOTween.Sequence();
        modSeq.Join(DOTween.To(() => oppRawScore, x => OppCenterText.text = x.ToString(), oppRealScore, _countTime).SetEase(Ease.InOutSine));
        modSeq.Join(DOTween.To(() => playerRawScore, x => PlayerCenterText.text = x.ToString(), playerRealScore, _countTime).SetEase(Ease.InOutSine));
        
        // Vừa giảm vừa giật nhẹ để có cảm giác bị ép
        modSeq.Join(OppCenterText.transform.DOShakePosition(_countTime, 10f));
        modSeq.Join(PlayerCenterText.transform.DOShakePosition(_countTime, 10f));
        yield return modSeq.WaitForCompletion();
        yield return new WaitForSeconds(0.5f);

        // bước 4: BAY VÀO TÔNG NHAU
        DG.Tweening.Sequence mergeSeq = DOTween.Sequence();
        mergeSeq.Join(OppCenterText.transform.DOMove(centerScreenPos, _mergeTime).SetEase(Ease.InBack));
        mergeSeq.Join(OppCenterText.transform.DOScale(Vector3.one * 1.5f, _mergeTime).SetEase(Ease.InBack));
        mergeSeq.Join(PlayerCenterText.transform.DOMove(centerScreenPos, _mergeTime).SetEase(Ease.InBack));
        mergeSeq.Join(PlayerCenterText.transform.DOScale(Vector3.one * 1.5f, _mergeTime).SetEase(Ease.InBack));
        yield return mergeSeq.WaitForCompletion();

        // bước 5: IMPACT
        PlayerCenterText.gameObject.SetActive(false);
        OppCenterText.gameObject.SetActive(false);
        FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor, _flashTime);
        Camera.main.transform.DOShakePosition(0.4f, 0.5f, 25);

        // bước 6: HIỆN CHÊNH LỆCH
        FinalDamageText.gameObject.SetActive(true);
        FinalDamageText.transform.position = centerScreenPos;
        FinalDamageText.alpha = 1f;
        FinalDamageText.transform.rotation = Quaternion.identity;

        if(damage == 0)
        {
            FinalDamageText.text = _drawLines[UnityEngine.Random.Range(0, _drawLines.Count)];
            FinalDamageText.color = Color.white;
            FinalDamageText.fontSize = 70;
        }
        else
        {
            FinalDamageText.text = iTakeDamage ? "-" + damage : damage.ToString();
            FinalDamageText.color = iTakeDamage ? FilterManager.Instance.HazardColor : FilterManager.Instance.AdvantageColor; 
            FinalDamageText.fontSize = 150;
        }
        
        FinalDamageText.transform.localScale = Vector3.zero;

        DG.Tweening.Sequence popSeq = DOTween.Sequence();
        popSeq.Append(FinalDamageText.transform.DOScale(Vector3.one * 2.5f, _flashTime).SetEase(Ease.OutBack));
        popSeq.Join(FinalDamageText.transform.DOPunchRotation(new Vector3(0, 0, UnityEngine.Random.Range(-25f, 25f)), _flashTime * 1.25f, vibrato: 6));
        yield return popSeq.WaitForCompletion();

        yield return new WaitForSeconds(0.5f);

        // bước 7: ATTACK
        if(damage > 0)
        {
            Vector3 targetPos = iTakeDamage ? PlayerAvatarTransform.position : OppAvatarTransform.position;
            Vector3 uiTargetPos = Camera.main.WorldToScreenPoint(targetPos);
            uiTargetPos.z = 0f;
            
            // Bay vút đi
            DG.Tweening.Sequence attackSeq = DOTween.Sequence();
            attackSeq.Append(FinalDamageText.transform.DOMove(uiTargetPos, _attackTime)
                .SetEase(Ease.InBack, 1.5f));
            attackSeq.Join(FinalDamageText.transform.DOScale(1.5f, _attackTime)
                .SetEase(Ease.InBack));
            attackSeq.Join(FinalDamageText.DOFade(0.7f, _attackTime)
                .SetEase(Ease.InExpo));
            yield return attackSeq.WaitForCompletion();

            FinalDamageText.gameObject.SetActive(false);

            // Nháy đỏ nếu mình ăn đấm
            if (iTakeDamage) 
            {
                FilterManager.Instance.FlashScreen(FilterManager.Instance.HazardColor, _flashTime);
                FilterManager.Instance.FlashVignette(FilterManager.Instance.HazardColor, 0.45f, 0.4f);
                Camera.main.transform.DOShakePosition(0.4f, 0.5f, 25);
            } 
            else 
            {
                // Đối thủ ăn đấm nháy trắng
                FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor, _flashTime);
                Camera.main.transform.DOShakePosition(0.2f, strength: 0.3f, vibrato: 15);
            }
        }
        else
        {
            DG.Tweening.Sequence drawSeq = DOTween.Sequence();
            drawSeq.Append(FinalDamageText.transform.DOShakePosition(duration: 0.4f, strength: 20f, vibrato: 15));
            drawSeq.AppendInterval(0.5f);
            drawSeq.Join(FinalDamageText.transform.DOMoveY(FinalDamageText.transform.position.y - 200f, 0.3f).SetEase(Ease.InQuad));
            drawSeq.Join(FinalDamageText.DOFade(0f, 0.3f).SetEase(Ease.InQuad));
            //drawSeq.Join(FinalDamageText.transform.DOScale(Vector3.one * 3f, _attackTime)); // Phình to rồi biến mất
            
            yield return drawSeq.WaitForCompletion();
            
            FinalDamageText.gameObject.SetActive(false);
        }

        //UpdateHpTexts(amIHost);
    }


    public void ShowCustomGameOver(string title, string message)
    {
        GameOverPanel.gameObject.SetActive(true);
        ResultText.text = title;
        MessageText.text = message;

        GameOverPanel.gameObject.SetActive(true);
        MenuButton.transform.localScale = Vector2.zero;
        MenuButton.GetComponent<Button>().enabled = false;
        ResultText.text = title;
        MessageText.text = message;

        TableVisualManager.Instance.StopAllCoroutines();
        FilterManager.Instance.SetDramaticFilter(true);

        GameOverPanel.alpha = 0f;
        ResultText.transform.localScale = Vector3.zero;
        MessageText.transform.localScale = Vector3.zero;

        Sequence gameOverSeq = DOTween.Sequence();
        gameOverSeq.Append(GameOverPanel.DOFade(1f, _gameOverAppearTime));
        gameOverSeq.Join(ResultText.transform.DOScale(Vector3.one, _gameOverAppearTime * 1.5f).SetEase(Ease.OutBack));
        gameOverSeq.Join(MessageText.transform.DOScale(Vector3.one, _gameOverAppearTime * 1.5f).SetEase(Ease.OutBack));
        gameOverSeq.Join(Camera.main.transform.DOShakePosition(0.5f, 0.5f, 20));

        gameOverSeq.AppendInterval(1.5f);

        MenuButton.gameObject.SetActive(true);
        gameOverSeq.Append(MenuButton.transform.DOScale(Vector3.one, 0.5f)).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                MenuButton.GetComponent<Button>().enabled = true;
            });
    }

    public void ShowGameOverUI(bool isHostWinner, int oldRank, int eloChange, int oldSouls, int earnedSouls)
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        bool didIWin = (amIHost && isHostWinner) || (!amIHost && !isHostWinner);
        string resultMessage = "";
        string message = "";

        if (didIWin)
        {
            resultMessage = "YOU WON";
            message = _winMessages[UnityEngine.Random.Range(0, _winMessages.Count)];
        }
        else
        {
            resultMessage = "YOU LOSE";
            message = _loseMessages[UnityEngine.Random.Range(0, _loseMessages.Count)];
        }

        GameOverPanel.gameObject.SetActive(true);
        MenuButton.transform.localScale = Vector2.zero;
        MenuButton.GetComponent<Button>().enabled = false;
        ResultText.text = resultMessage;
        MessageText.text = message;

        RankPointText.text = $"{oldRank}";
        SoulText.text = $"{oldSouls}";
        RankPointText.transform.localScale = Vector3.zero;
        SoulText.transform.localScale = Vector3.zero;

        TableVisualManager.Instance.StopAllCoroutines();
        FilterManager.Instance.SetDramaticFilter(true);

        GameOverPanel.alpha = 0f;
        ResultText.transform.localScale = Vector3.zero;
        MessageText.transform.localScale = Vector3.zero;

        Sequence gameOverSeq = DOTween.Sequence().SetUpdate(true);

        gameOverSeq.Append(GameOverPanel.DOFade(1f, _gameOverAppearTime));
        gameOverSeq.Join(ResultText.transform.DOScale(Vector3.one, _gameOverAppearTime).SetEase(Ease.OutBack));
        gameOverSeq.Join(MessageText.transform.DOScale(Vector3.one, _gameOverAppearTime).SetEase(Ease.OutBack));
        gameOverSeq.Join(Camera.main.transform.DOShakePosition(1f, 0.5f, 20));

        gameOverSeq.AppendInterval(0.5f);

        gameOverSeq.Append(RankPointText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));
        gameOverSeq.Join(SoulText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));

        gameOverSeq.AppendInterval(0.5f);

        // chạy số
        int currentVisualRank = oldRank;
        int targetRank = Mathf.Max(0, oldRank + eloChange);
        string rankPrefix = eloChange >= 0 ? "+" : ""; 
        string rankColor = eloChange >= 0 ? "#00FF00" : "#FF4444"; // Xanh lá nếu tăng, Đỏ nếu giảm

        gameOverSeq.Append(DOTween.To(() => currentVisualRank, x => {
            currentVisualRank = x;
            RankPointText.text = $"{currentVisualRank} <color={rankColor}>({rankPrefix}{eloChange})</color>";
        }, targetRank, 1.5f).SetEase(Ease.OutCubic));
        gameOverSeq.Append(RankPointText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, vibrato: 5));

        int currentVisualSoul = oldSouls;
        int targetSoul = oldSouls + earnedSouls;
        string soulPrefix = earnedSouls >= 0 ? "+" : "";
        string soulColor = "#00FFFF";

        gameOverSeq.AppendInterval(0.3f);

        // Dùng Join để cho Soul chạy ĐỒNG THỜI với Rank
        gameOverSeq.Append(DOTween.To(() => currentVisualSoul, x => {
            currentVisualSoul = x;
            SoulText.text = $"{currentVisualSoul} <color={soulColor}>({soulPrefix}{earnedSouls})</color>";
        }, targetSoul, 1.5f).SetEase(Ease.OutCubic));
        gameOverSeq.Append(SoulText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, vibrato: 5));


        gameOverSeq.AppendInterval(1.0f);

        MenuButton.gameObject.SetActive(true);
        gameOverSeq.Append(MenuButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

        gameOverSeq.OnComplete(() =>
        {
            MenuButton.GetComponent<Button>().enabled = true;
        });
    }


    public async void OnMenuButtonClickedAsync()
    {
        MenuButton.GetComponent<Button>().interactable = false;
        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        if (runner != null && !runner.IsShutdown)
        {
            //await runner.Shutdown();
            //Destroy(runner.gameObject);
            await runner.Shutdown(destroyGameObject: true);
        }
        SceneTransitionManager.Instance.LoadSceneAsync(GameConstants.SCENE_LOBBY);
    }
}

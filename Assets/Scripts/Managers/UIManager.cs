using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _bottomHpText;
    [SerializeField] private TextMeshProUGUI _topHpText;
    [SerializeField] private TextMeshProUGUI _bottomNameText;
    [SerializeField] private TextMeshProUGUI _topNameText;
    [SerializeField] private TextMeshPro _bottomStampCount;
    [SerializeField] private TextMeshPro _topStampCount;
    [Header("Seat Transforms")]
    [SerializeField] private Transform _bottomSeatTransform;
    [SerializeField] private Transform _topSeatTransform;

    [Header("Juice UI")]
    public CanvasGroup BlackScreenCurtain;
    public CanvasGroup BattleStartPanelGroup; 
    public RectTransform BattleStartTextRect;
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
            DontDestroyOnLoad(this.gameObject);
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
                hpText.color = new Color(1f, 0.5f, 0f); 
                
                // Phình to hơn bình thường nhưng không rung Camera
                hpText.transform.DOPunchScale(Vector3.one * 0.8f, 0.4f, vibrato: 15);
                
                // Lắc ngả nghiêng Text thay vì lắc vị trí (Tạo cảm giác bị gõ trúng đầu)
                hpText.transform.DOShakeRotation(0.4f, strength: new Vector3(0, 0, 20f), vibrato: 15);
            }

            // Trả về màu trắng sau khi giật xong
            hpText.DOColor(Color.white, 0.5f).SetDelay(0.5f);
        }
        else // NẾU ĐƯỢC HỒI MÁU
        {
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

    public void ShowBattleStartAnnouncement()
    {
        if (BattleStartPanelGroup == null || BattleStartTextRect == null) return;

        BattleStartPanelGroup.DOKill();
        BattleStartTextRect.DOKill();
        
        BattleStartPanelGroup.gameObject.SetActive(true);
        
        BattleStartPanelGroup.alpha = 0f;
        
        BattleStartTextRect.localScale = Vector3.one;
        BattleStartTextRect.anchoredPosition = new Vector2(-1500f, 0f);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);
        seq.Append(BattleStartPanelGroup.DOFade(1f, 0.5f));
        
        seq.Join(BattleStartTextRect.DOAnchorPosX(0f, _battleStartTextAppearDuration).SetEase(Ease.OutBack));
        //seq.Join(BattleStartTextRect.DOPunchScale(Vector3.one * 0.5f, _battleStartTextAppearDuration, vibrato: 10, elasticity: 1));

        if (Camera.main != null)
        {
            seq.Join(Camera.main.transform.DOShakePosition(0.5f, strength: 0.2f, vibrato: 15));
        }

        seq.AppendInterval(_battleStartTextDuration);
        seq.Append(BattleStartTextRect.DOAnchorPosX(2000f, _battleStartTextAppearDuration).SetEase(Ease.InBack));
        seq.Join(BattleStartPanelGroup.DOFade(0f, _battleStartTextAppearDuration * 0.8f));

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
            TurnAnnouncementTMP.text = $"MATCH FOUND"; 
            TurnAnnouncementRect.localScale = Vector3.one * 1.5f;
            TurnAnnouncementCanvasGroup.alpha = 0f; 
             // canvas mờ hiện leennn và text xuất hiện

            seq.Append(TurnAnnouncementCanvasGroup.DOFade(1f, 0.5f));
            seq.Join(TurnAnnouncementRect.DOScale(Vector3.one, _matchFoundTextAppearDuration).SetEase(Ease.OutBack));
            seq.AppendInterval(_matchFoundTextDuration);
            seq.Append(TurnAnnouncementCanvasGroup.DOFade(0f, 0.5f));

            seq.AppendCallback(() =>
            {
               TurnAnnouncementTMP.text = $"TURN {turnNumber}";
               TurnAnnouncementRect.localScale = Vector3.one * 3f; 
            });
        }
        else
        {
            TurnAnnouncementTMP.text = $"TURN {turnNumber}";
            TurnAnnouncementRect.localScale = Vector3.one * 3f;
            TurnAnnouncementCanvasGroup.alpha = 0f;
        }



        seq.Append(TurnAnnouncementRect.DOScale(Vector3.one, _turnTextAppearDuration).SetEase(Ease.OutExpo));
        seq.Join(TurnAnnouncementCanvasGroup.DOFade(1f, 0.5f));

        if (Camera.main != null)
        {
            seq.Join(Camera.main.transform.DOShakePosition(0.3f, strength: 0.1f, vibrato: 10));
        }

        seq.AppendInterval(_turnTextDuration);

        seq.Append(TurnAnnouncementRect.DOScale(Vector3.one * 1.5f, _turnTextAppearDuration).SetEase(Ease.InQuad));
        seq.Join(TurnAnnouncementCanvasGroup.DOFade(0f, 0.5f));

        if (BlackScreenCurtain != null)
        {
            seq.Join(BlackScreenCurtain.DOFade(0f, 0.5f).SetEase(Ease.InOutSine));
        }

        seq.OnComplete(() => {
            TurnAnnouncementRect.gameObject.SetActive(false);
            if (BlackScreenCurtain != null)
            {
                BlackScreenCurtain.gameObject.SetActive(false);
            }
        });
    }
}

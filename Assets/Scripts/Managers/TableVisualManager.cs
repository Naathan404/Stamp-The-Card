using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;

public class TableVisualManager : Singleton<TableVisualManager>
{
    #region VARIABLES & PROPERTIES
    [Header("Bottom Cards")]
    public SpriteRenderer[] BottomCardSprites = new SpriteRenderer[3];
    public TextMeshPro[] BottomCardTexts = new TextMeshPro[3];

    [Header("Top Cards")]
    public SpriteRenderer[] TopCardSprites = new SpriteRenderer[3];
    public TextMeshPro[] TopCardTexts = new TextMeshPro[3];

    [Header("Stamps")]
    public SpriteRenderer[] StampSprites = new SpriteRenderer[3];
    private Vector2[] _originalStampPosition = new Vector2[3];
    private Vector2[] _originalStampScale = new Vector2[3];

    [Header("Center Deck")]
    [SerializeField] private Transform _mainDeckTransform;

    [Header("Back Card")]
    [SerializeField] private Sprite CardBackSprite;

    [Header("Card Setting")]
    [SerializeField] private Vector2 _cardDealScale = new Vector2(1.8f, 1.8f);
    [SerializeField] private Vector3 _cardSpacing = new Vector3(1.5f, 0, 0);
    [SerializeField] private Vector2 _cardScale = new Vector2(2.5f, 2.5f);


    [Header("Animation Settings")]
    [SerializeField] private float _animCardSlideDuration = 0.4f;

    [Header("End Phase Cinematic UI")]
    public TextMeshProUGUI PlayerCenterText;
    public TextMeshProUGUI OppCenterText;
    public TextMeshProUGUI FinalDamageText;
    public Transform PlayerAvatarTransform; // Vị trí bay vào nếu mình thua
    public Transform OppAvatarTransform;    // Vị trí bay vào nếu địch thua


    [Header("Prefabs")]
    [SerializeField] private TextMeshPro _floatingTextPrefab;
    [SerializeField] private SpriteRenderer _stampHologramPrefab;

    private Vector3[] _bottomInitialPos = new Vector3[3];
    private Vector3[] _topInitialPos = new Vector3[3];
    private Queue<TextMeshPro> _floatingTextPool = new Queue<TextMeshPro>();
    private Coroutine _dealCoroutine;


    private float _rotateOpponentCardTime = 0.3f;
    private float _stampResolveTime = 0.4f;
    private float _hologramFlashTime = 0.3f;
    private float _hologramFloatingTime = 0.5f;

    Color grayColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    #endregion

    private void Start()
    {
        for(int i = 0; i < 3; i++)
        {
            _bottomInitialPos[i] = BottomCardSprites[i].transform.position;
            _topInitialPos[i] = TopCardSprites[i].transform.position;

            BottomCardSprites[i].gameObject.SetActive(false);
            TopCardSprites[i].gameObject.SetActive(false);
            StampSprites[i].gameObject.SetActive(false);

            _originalStampPosition[i] = StampSprites[i].transform.position;
            _originalStampScale[i] = StampSprites[i].transform.localScale;
        }
    }

    public void RenderCardOnTable(int slotIndex, CardData cardData, bool isHostCard)
    {
        bool isHost = GameManager.Instance.Runner.IsServer;
        bool isMyCard = (isHost && isHostCard) || (!isHost && !isHostCard);

        if(isMyCard)
        {
            BottomCardSprites[slotIndex].sprite = cardData.Artwork;
            BottomCardTexts[slotIndex].text = cardData.BaseScore.ToString();
        }
        else
        {
            TopCardSprites[slotIndex].sprite = cardData.Artwork;
            TopCardTexts[slotIndex].text = cardData.BaseScore.ToString();
        }
    }

    public void PlayDealAnimation(CardData[] hostCards, CardData[] clientCards)
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        // Phân định trên dưới
        CardData[] myCards = amIHost ? hostCards : clientCards;
        CardData[] oppCards = amIHost ? clientCards : hostCards;
        SetActiveSpritesOnTable(false);

        if (_dealCoroutine != null) StopCoroutine(_dealCoroutine);
        _dealCoroutine = StartCoroutine(DealCardsRoutine(myCards, oppCards));
    }

    public void StartCalculatePhaseVisuals()
    {
        StartCoroutine(CalculatePhaseMasterRoutine());
    }

    private IEnumerator CalculatePhaseMasterRoutine()
    {
        CardSlot[] hostSlots = GetHostCardSlots();
        CardSlot[] clientSlots = GetClientCardSlots();
        for (int i = 0; i < 3; i++)
        {
            if (hostSlots[i].Data != null)
            {
                hostSlots[i].Reset();
                if (GameManager.Instance.IsCardStampsPermanentlyDisabled(hostSlots[i].Data.CardID))
                {
                    hostSlots[i].StampsDisabled = true;
                }
            }
            if (clientSlots[i].Data != null)
            {
                clientSlots[i].Reset();
                if (GameManager.Instance.IsCardStampsPermanentlyDisabled(clientSlots[i].Data.CardID))
                {
                    clientSlots[i].StampsDisabled = true;
                }
            }
        }

        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(RevealOpponentCardRoutine());

        int currentTurn = GameStateManager.Instance.CurrentTurn;

        // resolve stamp Tier 0 
        Debug.Log("[Table Visual] Resolve Các stamp Tier 0...");
        yield return StartCoroutine(VisualResolveTierRoutine(ExecutionTier.Tier0_RuleSetting, hostSlots, clientSlots, currentTurn));
        
        // Resolve các stamp Tier 1 - 4
        Debug.Log("[Table Visual] Resolve Các stamp Tier 1 - 2 - 3 - 4...");
        yield return StartCoroutine(VisualResolveMainStampsRoutine(hostSlots, clientSlots, currentTurn));

        yield return new WaitForSeconds(2f);
        // Nảy máu, trừ HP...
        if(GameManager.Instance.Runner.IsServer)
        {
            GameManager.Instance.RPC_SyncAndShowScores(
                hostSlots[0].Score, hostSlots[1].Score, hostSlots[2].Score,
                clientSlots[0].Score, clientSlots[1].Score, clientSlots[2].Score
            );

            Debug.Log("[Table Visual] Báo hiệu chuyển sang Endphase và Visualize HP...");
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.EndPhase);
        }
    }

    #region DUEL CARDS
    private IEnumerator DealCardsRoutine(CardData[] myCards, CardData[] oppCards)
    {
        if (GameStateManager.Instance.CurrentTurn == 1)
        {
            yield return new WaitForSeconds(4f);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        // kill hết anim đang chạy
        for(int i = 0; i < 3; i++) 
        {
            BottomCardSprites[i].transform.DOKill(true);
            TopCardSprites[i].transform.DOKill(true);
            
            BottomCardTexts[i].transform.DOKill(true);
            TopCardTexts[i].transform.DOKill(true);
            
            BottomCardTexts[i].transform.localScale = Vector3.one;
            TopCardTexts[i].transform.localScale = Vector3.one;
        }

        Vector3 safeDealScale = new Vector3(_cardDealScale.x, _cardDealScale.y, 1f);
        Vector3 safeCardScale = new Vector3(_cardScale.x, _cardScale.y, 1f);

        for (int i = 0; i < 3; i++)
        {
            if (GameManager.Instance == null || this == null) 
            {
                yield break;
            }
            
            // set up 
            BottomCardSprites[i].gameObject.SetActive(true);
            BottomCardTexts[i].gameObject.SetActive(true);
            BottomCardSprites[i].transform.position = _mainDeckTransform.position;
            BottomCardSprites[i].transform.rotation = Quaternion.Euler(0, 0, 90f);
            BottomCardSprites[i].sprite = CardBackSprite;
            BottomCardTexts[i].text = ""; 
            BottomCardSprites[i].transform.localScale = Vector3.zero;

            TopCardSprites[i].gameObject.SetActive(true);
            TopCardTexts[i].gameObject.SetActive(false);
            TopCardSprites[i].transform.position = _mainDeckTransform.position;
            TopCardSprites[i].transform.rotation = Quaternion.Euler(0, 0, 90f);
            TopCardSprites[i].sprite = CardBackSprite;
            TopCardTexts[i].text = ""; 
            TopCardSprites[i].transform.localScale = Vector3.zero;

            /// xóa stamp trên mỗi lá
            CardSlot bottomSlot = BottomCardSprites[i].GetComponent<CardSlot>();
            CardSlot topSlot = TopCardSprites[i].GetComponent<CardSlot>();
            for(int s = 0; s < 3; s++)
            {
                if(bottomSlot != null && bottomSlot.StampRenderers[s] != null)
                {
                    bottomSlot.StampRenderers[s].gameObject.SetActive(false);
                    bottomSlot.StampRenderers[s].enabled = false;
                }
                if (topSlot != null && topSlot.StampRenderers[s] != null)
                {
                    topSlot.StampRenderers[s].gameObject.SetActive(false);
                    topSlot.StampRenderers[s].enabled = false;
                }
            }

            // phóng bài
            // BottomCardSprites[i].transform.DOScale(_cardDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
            // BottomCardSprites[i].transform.DORotate(Vector3.zero, _animCardSlideDuration).SetEase(Ease.OutBack);
            // BottomCardSprites[i].transform.DOMove(_bottomInitialPos[i], _animCardSlideDuration).SetEase(Ease.OutQuad);

            // TopCardSprites[i].transform.DOScale(_cardDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
            // TopCardSprites[i].transform.DORotate(Vector3.zero, _animCardSlideDuration).SetEase(Ease.OutBack);
            // TopCardSprites[i].transform.DOMove(_topInitialPos[i], _animCardSlideDuration).SetEase(Ease.OutQuad);
            BottomCardSprites[i].transform.DOScale(safeDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
            BottomCardSprites[i].transform.DORotate(Vector3.zero, _animCardSlideDuration).SetEase(Ease.OutBack);
            BottomCardSprites[i].transform.DOMove(_bottomInitialPos[i], _animCardSlideDuration).SetEase(Ease.OutQuad);

            TopCardSprites[i].transform.DOScale(safeDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
            TopCardSprites[i].transform.DORotate(Vector3.zero, _animCardSlideDuration).SetEase(Ease.OutBack);
            TopCardSprites[i].transform.DOMove(_topInitialPos[i], _animCardSlideDuration).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(_animCardSlideDuration);

            // lật bài
            int index = i; 
            
            BottomCardSprites[index].transform.DOScaleX(0f, 0.15f).OnComplete(() => 
            {
                BottomCardSprites[index].sprite = myCards[index].Artwork;
                BottomCardTexts[index].text = myCards[index].BaseScore.ToString();

                CardSlot slot = BottomCardSprites[index].GetComponent<CardSlot>();
                if (slot != null) 
                {
                    slot.Data = myCards[index]; // Gán CardData
                }

                // BottomCardSprites[index].transform.DOScale(safeCardScale, 0.15f).SetEase(Ease.OutBack).OnComplete(() => 
                // {
                //     // vẽ stamp lên card
                //     RenderStampsOnBoard(GameManager.Instance.CardAttachedStamps);
                // });
                BottomCardSprites[index].transform.DOScale(safeCardScale, 0.15f).SetEase(Ease.OutBack);
            });

            TopCardSprites[index].transform.DOScaleX(0f, 0.15f).OnComplete(() => 
            {
                // KHÔNG đổi sprite sang Artwork, KHÔNG hiện Text điểm số
                CardSlot slot = TopCardSprites[index].GetComponent<CardSlot>();
                if (slot != null) slot.Data = oppCards[index]; // Vẫn âm thầm gán Data để lát tính điểm

                TopCardSprites[index].transform.DOScale(safeCardScale, 0.15f).SetEase(Ease.OutBack);
            });

            yield return new WaitForSeconds(0.3f); 
        }

        RenderStampsOnBoard(GameManager.Instance.CardAttachedStamps);
        yield return new WaitForSeconds(1.0f); 

        // chuyển sang main phase
        if (GameManager.Instance.Runner.IsServer)
        {
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.MainPhase);
        }
    }
    #endregion

    #region DRAW STAMPS ON CARDS
    private void DrawStampsForPlayer(NetworkArray<int> playerHand, SpriteRenderer[] visualSlots, NetworkArray<int> allStamps)
    {
        for (int slotIndex = 0; slotIndex < 3; slotIndex++)
        {
            int cardID = playerHand[slotIndex];
            if (cardID == -1) continue; 

            CardSlot cardSlot = visualSlots[slotIndex].GetComponent<CardSlot>();
            if (cardSlot == null) continue;

            bool isJoker = GameConstants.IsJokerStamp(cardID);
            int dataLimit = isJoker ? 1 : 3;
            int startIndex = cardID * 3; 

            for (int i = 0; i < dataLimit; i++)
            {
                int stampID = allStamps[startIndex + i];
                int visualIndex = isJoker ? 1 : i;
                SpriteRenderer stampRenderer = cardSlot.StampRenderers[visualIndex];

                if (stampID > 0) 
                {
                    // chạy anim cho mấy stamp chưa được bật
                    if (!stampRenderer.enabled)
                    {
                        Vector3 originalScale = stampRenderer.transform.localScale;
                        stampRenderer.enabled = true;
                        stampRenderer.gameObject.SetActive(true);

                        stampRenderer.sprite = DataManager.Instance.GetStampDataByID(stampID).stampArt;

                        Transform stampTransform = stampRenderer.transform;
                        stampTransform.rotation = Quaternion.identity;
                        stampTransform.DOKill();
                        
                        stampTransform.localScale = Vector3.one;
                        stampTransform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
                        stampTransform.GetComponent<SpriteRenderer>().DOFade(1f, 0.25f).SetEase(Ease.OutExpo);
                        stampTransform.DOScale(originalScale, 0.25f).SetEase(Ease.InExpo).OnComplete(() => 
                        {
                            stampTransform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                            cardSlot.transform.DOShakePosition(0.2f, strength: new Vector3(0.15f, 0.15f, 0f), vibrato: 15);
                            cardSlot.transform.DOShakeRotation(0.2f, strength: new Vector3(0, 0, 3f), vibrato: 10);
                        });
                    }
                }
                else
                {
                    // Nếu không có tem thì tắt hiển thị
                    stampRenderer.enabled = false;
                }
            }
        }
    }
    #endregion

    #region HELPERS
    public void SpawnStampChoices(NetworkArray<int> stampIDs, bool isHostChoice)
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        if (isHostChoice != amIHost) return;

        for (int i = 0; i < 3; i++)
        {
            int sID = stampIDs[i];
            StampSprites[i].transform.DOKill();

            if (sID > 0)
            {
                var dragger = StampSprites[i].GetComponent<StampDragger>();
                if (dragger != null)
                {
                    dragger.stampID = sID;
                    dragger.isUsed = false;
                }
                var sr = StampSprites[i].GetComponent<SpriteRenderer>();
                sr.enabled = true;
                sr.DOFade(1f, 0f);

                StampSprites[i].sprite = DataManager.Instance.GetStampDataByID(sID).stampArt;
                StampSprites[i].transform.DOKill();
                StampSprites[i].transform.position = _originalStampPosition[i];
                StampSprites[i].gameObject.SetActive(true);
                StampSprites[i].transform
                    .DOScale(_originalStampScale[i], 0.4f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.1f);
            }
            else
            {
                StampSprites[i].gameObject.SetActive(false);
            }
        }
    }

    public void RenderStampsOnBoard(NetworkArray<int> allStamps)
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        if (amIHost)
        {
            DrawStampsForPlayer(GameManager.Instance.HostHand, BottomCardSprites, allStamps);
        }
        else
        {
            DrawStampsForPlayer(GameManager.Instance.ClientHand, BottomCardSprites, allStamps);
        }
    }

    public void HideAllStamps()
    {
        for(int i = 0; i < 3; i++)
        {
            if(StampSprites[i] != null && StampSprites[i].gameObject.activeSelf)
            {
                var dragger = StampSprites[i].GetComponent<StampDragger>();

                int capturedIndex = i; 
                StampSprites[capturedIndex].transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        StampSprites[capturedIndex].gameObject.SetActive(false);
                    });
            }
        }
    }

    /// Hàm chạy hiệu ứng ẩn stamps
    public void HideUnusedStamps(GameObject usedStampGO)
    {
        for (int i = 0; i < 3; i++)
        {
            if (StampSprites[i].gameObject != usedStampGO && StampSprites[i].gameObject.activeSelf)
            {
                var dragger = StampSprites[i].GetComponent<StampDragger>();
                dragger.isUsed = true;
                int capturedIndex = i;  
                StampSprites[capturedIndex].transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        StampSprites[capturedIndex].gameObject.SetActive(false);
                    });
            }
        }
    }

    public void UpdateBoardScores()
    {
        for (int i = 0; i < 3; i++)
        {
            CardSlot bottomSlot = BottomCardSprites[i].GetComponent<CardSlot>();
            if (bottomSlot != null) 
            {
                AnimateScoreText(BottomCardTexts[i], bottomSlot.Score);
            }

            CardSlot topSlot = TopCardSprites[i].GetComponent<CardSlot>();
            if (topSlot != null) 
            {
                AnimateScoreText(TopCardTexts[i], topSlot.Score);
            }
        }
    }

    public CardSlot[] GetHostCardSlots()
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        SpriteRenderer[] slots = amIHost ? BottomCardSprites : TopCardSprites;
        CardSlot[] result = new CardSlot[3];
        for (int i = 0; i < 3; i++)
            result[i] = slots[i].GetComponent<CardSlot>();
        return result;
    }

    public CardSlot[] GetClientCardSlots()
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        SpriteRenderer[] slots = amIHost ? TopCardSprites : BottomCardSprites;
        CardSlot[] result = new CardSlot[3];
        for (int i = 0; i < 3; i++)
            result[i] = slots[i].GetComponent<CardSlot>();
        return result;
    }

    private TextMeshPro GetTextForSlot(CardSlot slot)
    {
        for (int i = 0; i < 3; i++)
        {
            // Kiểm tra xem slot này nằm ở hàng dưới hay hàng trên
            if (BottomCardSprites[i].GetComponent<CardSlot>() == slot) return BottomCardTexts[i];
            if (TopCardSprites[i].GetComponent<CardSlot>() == slot) return TopCardTexts[i];
        }
        return null;
    }

    private void SetActiveSpritesOnTable(bool isActive)
    {
        foreach (var card in TopCardSprites)
            card.gameObject.SetActive(isActive);
        foreach (var card in BottomCardSprites)
            card.gameObject.SetActive(isActive);
        
        foreach (var text in TopCardTexts)
            text.gameObject.SetActive(isActive);
        foreach (var text in BottomCardTexts)
            text.gameObject.SetActive(isActive);
    }
    #endregion

    #region ANIMATIONS
    private void AnimateScoreText(TextMeshPro textMesh, int targetScore)
    {
        int currentScore = targetScore;
        int.TryParse(textMesh.text, out currentScore);

        if (currentScore == targetScore) 
        {
            textMesh.text = targetScore.ToString();
            return;
        }

        // textMesh.transform.DOKill(true); 
        // textMesh.transform.DOPunchScale(Vector3.one * 0.4f, 0.5f, vibrato: 3);

        // DOTween.To(() => currentScore, x => {
        //     textMesh.text = x.ToString();
        // }, targetScore, 0.5f).SetEase(Ease.OutQuad);
        int difference = targetScore - currentScore;
        SpawnFloatingText(textMesh, difference);

        textMesh.transform.DOKill(true); 
        textMesh.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, vibrato: 3);

        DOTween.To(() => currentScore, x => {
            textMesh.text = x.ToString();
        }, targetScore, 0.5f).SetEase(Ease.OutQuad);
    }

    private void ShowFloatingTextCore(TextMeshPro referenceText, string message, Color textColor, float offsetX = 0f)
    {
        if (_floatingTextPrefab == null) return;

        TextMeshPro tmp;
        if (_floatingTextPool.Count > 0)
        {
            tmp = _floatingTextPool.Dequeue();
            tmp.gameObject.SetActive(true);
        }
        else
        {
            tmp = Instantiate(_floatingTextPrefab);
        }

        tmp.transform.DOKill(); 
        
        tmp.text = message; 
        tmp.color = new Color(textColor.r, textColor.g, textColor.b, 1f); 
        tmp.sortingOrder = 30000; 
        
        Vector3 spawnPos = referenceText.transform.position + new Vector3(offsetX, -1.5f, 0f);
        tmp.transform.position = spawnPos;

        tmp.transform.DOMoveY(spawnPos.y + 3f, 2f).SetEase(Ease.OutCirc);
        if (offsetX != 0)
        {
            tmp.transform.DOMoveX(spawnPos.x + (offsetX * 0.5f), 2f).SetEase(Ease.OutCirc);
        }
        
        tmp.DOFade(0f, 1f).SetDelay(1f).OnComplete(() => 
        {
            tmp.gameObject.SetActive(false);     
            _floatingTextPool.Enqueue(tmp);     
        });
    }

    private void SpawnFloatingText(TextMeshPro referenceText, int diff)
    {
        string msg = diff > 0 ? $"+{diff}" : $"{diff}"; 
        Color targetColor = diff > 0 ? Color.green : Color.red;
        ShowFloatingTextCore(referenceText, msg, targetColor, -1f);
    }

    public void ShowMissText(TextMeshPro referenceText, string customMessage = "VÔ HIỆU")
    {
        ShowFloatingTextCore(referenceText, customMessage, grayColor);
        referenceText.transform.DOKill(true);
        referenceText.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.4f, vibrato: 6);
    }
    #endregion


    #region RESOLVE ANIMATIONS
    public IEnumerator RevealOpponentCardRoutine()
    {
        Debug.Log("[Visual] Bắt đầu lật bài đối thủ...");
        bool amIHost = GameManager.Instance.Runner.IsServer;

        var oppHand = amIHost ? GameManager.Instance.ClientHand : GameManager.Instance.HostHand;
        var oppStamps = GameManager.Instance.CardAttachedStamps;

        for(int i = 0; i < 3; i++)
        {
            SpriteRenderer topCard = TopCardSprites[i];
            CardSlot topslot = topCard.GetComponent<CardSlot>();

            int cardID = oppHand[i];
            //int cardID = oppHand[2 - i];
            if(cardID == -1) continue;

            CardData cardData = DataManager.Instance.GetCardDataByID(cardID);

            topCard.transform.DOMoveY(topCard.transform.position.y - 0.72f, _rotateOpponentCardTime);
            topCard.transform.DORotate(new Vector3(0, 90, 0), _rotateOpponentCardTime).SetEase(Ease.InQuad);

            yield return new WaitForSeconds(_rotateOpponentCardTime);

            topCard.sprite = cardData.Artwork;
            topslot.Data = cardData;
            topslot.Score = cardData.BaseScore;
            topCard.transform.DORotate(Vector3.zero, _rotateOpponentCardTime).SetEase(Ease.OutQuad);

            TopCardTexts[i].gameObject.SetActive(true);
            TopCardTexts[i].text = topslot.Score.ToString();
            TopCardTexts[i].transform.DOPunchScale(Vector3.one * 0.5f, _rotateOpponentCardTime);
            yield return new WaitForSeconds(0.2f);
        }

        for(int i = 0; i < 3; i++)
        {
            int cardID = oppHand[i];
            //int cardID = oppHand[2 - i];
            if (cardID == -1) continue;

            CardSlot topSlot = TopCardSprites[i].GetComponent<CardSlot>();
            int startIndex = cardID * 3; 

            bool isJoker = GameConstants.IsJokerStamp(cardID);
            int dataLimit = isJoker ? 1 : 3;

            for (int s = 0; s < dataLimit; s++)
            {
                int stampID = oppStamps[startIndex + s];
                int visualIndex = isJoker ? 1 : s;
                SpriteRenderer stampRenderer = topSlot.StampRenderers[visualIndex];

                if (stampID > 0) 
                {
                    // Ép bật Object và Component
                    stampRenderer.gameObject.SetActive(true);
                    stampRenderer.enabled = true;
                    stampRenderer.color = Color.white;
                    stampRenderer.sortingOrder = TopCardSprites[i].sortingOrder + 1;
                    Vector2 originalScale = stampRenderer.transform.localScale;

                    // Kéo Z ra trước mặt bài để chống lún
                    Vector3 localPos = stampRenderer.transform.localPosition;
                    localPos.z = -0.1f;
                    stampRenderer.transform.localPosition = localPos;

                    // Gán Data Tem vào UI
                    stampRenderer.sprite = DataManager.Instance.GetStampDataByID(stampID).stampArt;

                    // Animation nện xuống
                    Transform stampTransform = stampRenderer.transform;
                    
                    stampTransform.DOKill(); 
                    stampTransform.localScale = originalScale * 3f; 
                    
                    stampTransform.DOScale(originalScale, 0.25f).SetEase(Ease.InExpo).OnComplete(() => 
                    {
                        TopCardSprites[i].transform.DOShakePosition(0.2f, strength: new Vector3(0.15f, 0.15f, 0f), vibrato: 15);
                    });
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("[Table Visual Manager] Đã lật xong bài địch");
    }

    private IEnumerator VisualResolveTierRoutine(ExecutionTier tier, CardSlot[] hostSlots, CardSlot[] clientSlots, int currentTurn)
    {
        bool hostFirst = (currentTurn % 2 == 1);
        if (hostFirst) 
        {
            yield return StartCoroutine(ApplyStampsVisualRoutine(tier, hostSlots, clientSlots, false));
            yield return StartCoroutine(ApplyStampsVisualRoutine(tier, clientSlots, hostSlots, false));
        } 
        else 
        {
            yield return StartCoroutine(ApplyStampsVisualRoutine(tier, clientSlots, hostSlots, false));
            yield return StartCoroutine(ApplyStampsVisualRoutine(tier, hostSlots, clientSlots, false));
        }
    }

    private IEnumerator VisualResolveMainStampsRoutine(CardSlot[] hostSlots, CardSlot[] clientSlots, int currentTurn)
    {
        bool hostFirst = currentTurn % 2 == 1;
        if (hostFirst) 
        {
            yield return StartCoroutine(ApplyStampsVisualRoutine(null, hostSlots, clientSlots, true));
            yield return StartCoroutine(ApplyStampsVisualRoutine(null, clientSlots, hostSlots, true));
        } 
        else 
        {
            yield return StartCoroutine(ApplyStampsVisualRoutine(null, clientSlots, hostSlots, true));
            yield return StartCoroutine(ApplyStampsVisualRoutine(null, hostSlots, clientSlots, true));
        }
    }

    private IEnumerator ApplyStampsVisualRoutine(ExecutionTier? tier, CardSlot[] mySlots, CardSlot[] oppSlots, bool isMainTier)
    {
        for(int i = 0; i < 3; i++)
        {
            if(mySlots[i].Data == null || mySlots[i].IsIgnored || mySlots[i].StampsDisabled) continue;

            int cardID = mySlots[i].Data.CardID;
            int startIndex = cardID * 3;
            Debug.Log($"[Soi Data] Lá bài ID {cardID} đang chứa 3 Tem: {GameManager.Instance.CardAttachedStamps[startIndex]} | {GameManager.Instance.CardAttachedStamps[startIndex+1]} | {GameManager.Instance.CardAttachedStamps[startIndex+2]}");

            for (int s = 0; s < 3; s++)
            {
                int stampID = GameManager.Instance.CardAttachedStamps[startIndex + s];
                if (stampID > 0)
                {
                    BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);
                    if(stampData != null && stampData.isEnabled)
                    {
                        bool matchTier = false;
                        if (!isMainTier && stampData.ExeTier == tier) 
                            matchTier = true;
                        if (isMainTier && stampData.ExeTier != ExecutionTier.Tier0_RuleSetting)
                            matchTier = true;

                        if (matchTier)
                        {
                            //yield return StartCoroutine(AnimateStampTrigger(mySlots[i], s, cardID));
                            yield return StartCoroutine(AnimateStampTrigger(mySlots[i], s, stampData));

                            string effectMessage = stampData.ApplyEffect(mySlots, oppSlots, i);
                            TextMeshPro targetText = GetTextForSlot(mySlots[i]);

                            //bool hasEffect = stampData.ApplyEffect(mySlots, oppSlots, i);

                            if (effectMessage == "ScoreChanged")
                            {
                                UpdateBoardScores();
                            }
                            else if (!string.IsNullOrEmpty(effectMessage) && effectMessage != "NoEffect")
                            {
                                UpdateBoardScores();
                                if (targetText != null)
                                {
                                    ShowFloatingTextCore(targetText, effectMessage, grayColor, 1f); 
                                }
                            }
                            else
                            {
                                if (targetText != null)
                                {
                                    ShowMissText(targetText, "No effect"); 
                                }
                            }

                            yield return new WaitForSeconds(0.6f);
                        }             
                    }
                }
            }
        }
    }

    private IEnumerator AnimateStampTrigger(CardSlot slot, int stampIndex, BaseStampData stampData)
    {
        int cardID = slot.Data.CardID;
        bool isJoker = GameConstants.IsJokerStamp(cardID);
        int visualIndex = isJoker ? 1 : stampIndex;

        SpriteRenderer visual = slot.StampRenderers[visualIndex];

        if (visual != null && visual.gameObject.activeSelf)
        {
            // chớp sáng stamp goocs
            visual.transform.DOKill();
            visual.transform.DOPunchScale(Vector3.one * 0.6f, _stampResolveTime, vibrato: 10);
            visual.color = new Color(2f, 2f, 2f); 
            visual.DOColor(Color.white, _stampResolveTime);

            // gọi hologram
            if (_stampHologramPrefab != null)
            {
                Vector3 spawnPos = visual.transform.position + new Vector3(0, 0.5f, -2f);
                SpriteRenderer holo = Instantiate(_stampHologramPrefab, spawnPos, Quaternion.identity);
                
                holo.sprite = stampData.stampArt;
                holo.material = visual.material;

                // Lấy Scale thực tế của tem gốc làm chuẩn, rồi x2.5 lên cho to chà bá
                Vector3 baseScale = visual.transform.lossyScale; 
                
                // Hoạt ảnh: Bung từ số 0, bay lên cao và mờ dần
                holo.transform.localScale = Vector3.zero;
                // holo.transform.DOScale(baseScale * 2.5f, 0.3f).SetEase(Ease.OutBack); 
                holo.transform.DOScale(new Vector3(baseScale.x * 2f, baseScale.y * 3f, 1f), _stampResolveTime)
                    .OnComplete(() => {
                        holo.transform.DOScale(baseScale * 3.6f, _hologramFlashTime).SetEase(Ease.OutBack);
                    });
                holo.transform.DOMoveY(holo.transform.position.y + 2f, _hologramFloatingTime).SetEase(Ease.OutQuad);
                
                holo.DOFade(0f, _hologramFloatingTime).SetDelay(0.3f).OnComplete(() => Destroy(holo.gameObject));
            }
        }

        // shake cam
        if (Camera.main != null) 
        {
            Camera.main.transform.DOComplete();
            Camera.main.transform.DOShakePosition(0.2f, strength: 0.1f, vibrato: 10);
        }

        yield return new WaitForSeconds(0.5f); 
    }
    #endregion
}

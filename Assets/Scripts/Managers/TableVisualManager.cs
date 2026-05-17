using System.Collections;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TableVisualManager : Singleton<TableVisualManager>
{
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

    private Vector3[] _bottomInitialPos = new Vector3[3];
    private Vector3[] _topInitialPos = new Vector3[3];

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

        StartCoroutine(DealCardsRoutine(myCards, oppCards));
    }

    private IEnumerator DealCardsRoutine(CardData[] myCards, CardData[] oppCards)
    {
        // kill hết anim đang chạy
        for(int i = 0; i < 3; i++) {
            BottomCardSprites[i].transform.DOKill();
            TopCardSprites[i].transform.DOKill();
        }

        for (int i = 0; i < 3; i++)
        {
            // set up 
            BottomCardSprites[i].gameObject.SetActive(true);
            BottomCardSprites[i].transform.position = _mainDeckTransform.position;
            BottomCardSprites[i].transform.rotation = Quaternion.Euler(0, 0, 90f);
            BottomCardSprites[i].sprite = CardBackSprite;
            BottomCardTexts[i].text = ""; 
            BottomCardSprites[i].transform.localScale = Vector3.zero;

            TopCardSprites[i].gameObject.SetActive(true);
            TopCardSprites[i].transform.position = _mainDeckTransform.position;
            TopCardSprites[i].transform.rotation = Quaternion.Euler(0, 0, 90f);
            TopCardSprites[i].sprite = CardBackSprite;
            TopCardTexts[i].text = ""; 
            TopCardSprites[i].transform.localScale = Vector3.zero;

            // phóng bài
            BottomCardSprites[i].transform.DOScale(_cardDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
            BottomCardSprites[i].transform.DORotate(Vector3.zero, _animCardSlideDuration).SetEase(Ease.OutBack);
            BottomCardSprites[i].transform.DOMove(_bottomInitialPos[i], _animCardSlideDuration).SetEase(Ease.OutQuad);

            TopCardSprites[i].transform.DOScale(_cardDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
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
                if (slot != null) {
                    slot.Data = myCards[index]; // Gán CardData
                }


                BottomCardSprites[index].transform.DOScale(_cardScale, 0.15f).SetEase(Ease.OutBack).OnComplete(() => 
                {
                    // vẽ stamp lên card
                    RenderStampsOnBoard(GameManager.Instance.CardAttachedStamps);
                });
            });

            TopCardSprites[index].transform.DOScaleX(0f, 0.15f).OnComplete(() => 
            {
                // KHÔNG đổi sprite sang Artwork, KHÔNG hiện Text điểm số
                CardSlot slot = TopCardSprites[index].GetComponent<CardSlot>();
                if (slot != null) slot.Data = oppCards[index]; // Vẫn âm thầm gán Data để lát tính điểm

                TopCardSprites[index].transform.DOScale(_cardScale, 0.15f).SetEase(Ease.OutBack);
            });

            yield return new WaitForSeconds(0.3f); 
        }

        // chuyển sang main phase
        if (GameManager.Instance.Runner.IsServer)
        {
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.MainPhase);
        }
    }

    // vẽ stamp lên màn hình
    private void DrawStampsForPlayer(NetworkArray<int> playerHand, SpriteRenderer[] visualSlots, NetworkArray<int> allStamps)
    {
        for (int slotIndex = 0; slotIndex < 3; slotIndex++)
        {
            int cardID = playerHand[slotIndex];
            if (cardID == -1) continue;

            CardSlot cardSlot = visualSlots[slotIndex].GetComponent<CardSlot>();
            if (cardSlot == null) continue;

            if (cardSlot.StampRenderers == null || cardSlot.StampRenderers.Count < 3)
            {
                Debug.LogWarning($"[TableVisualManager] CardSlot tại slot {slotIndex} thiếu StampRenderers!");
                continue;
            }

            int stampCount = GameConstants.IsJokerStamp(cardID) ? 1 : 3;
            int startIndex = cardID * 3;

            for (int i = 0; i < 3; i++)
            {
                // Ô này nằm ngoài giới hạn stamp của lá bài như lá Joker thì tắt renderer
                if (i >= stampCount)
                {
                    cardSlot.StampRenderers[i].enabled = false;
                    continue;
                }

                int stampID = allStamps[startIndex + i];

                if (stampID > 0)
                {
                    BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);
                    if (stampData == null) continue;

                    cardSlot.StampRenderers[i].sprite = stampData.stampArt;
                    cardSlot.StampRenderers[i].enabled = true;
                }
                else
                {
                    cardSlot.StampRenderers[i].enabled = false;
                }
            }
        }
    }

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

                StampSprites[i].sprite = DataManager.Instance.GetStampDataByID(sID).stampArt;

                StampSprites[i].transform.position   = _originalStampPosition[i];
                StampSprites[i].transform.localScale  = _originalStampScale[i];
                StampSprites[i].gameObject.SetActive(true);

                StampSprites[i].transform
                    .DOScale(dragger.OriginalScale, 0.4f)
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


    /// Hàm chạy hiệu ứng ẩn stamps
    public void HideUnusedStamps(GameObject usedStampGO)
    {
        for (int i = 0; i < 3; i++)
        {
            if (StampSprites[i].gameObject != usedStampGO && StampSprites[i].gameObject.activeSelf)
            {
                var dragger = StampSprites[i].GetComponent<StampDragger>();
                if (dragger != null) dragger.isUsed = true;

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

    public void UpdateBoardScores(CardSlot[] hostSlots, CardSlot[] clientSlots)
    {
        for (int i = 0; i < 3; i++)
        {
            // Host Score UI
            BottomCardTexts[i].text = hostSlots[i].Score.ToString();
            BottomCardTexts[i].transform.DOPunchScale(Vector3.one * 0.2f, 0.2f); // Nảy số

            // Client Score UI
            TopCardTexts[i].text = clientSlots[i].Score.ToString();
            TopCardTexts[i].transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
    }

    // public CardSlot[] GetBottomCardSlots()
    // {
    //     CardSlot[] res = new CardSlot[3];
    //     for(int i = 0; i < 3; i++)
    //     {
    //         res[i] = BottomCardSprites[i].gameObject.GetComponent<CardSlot>();
    //     }
    //     return res;
    // }

    // public CardSlot[] GetTopCardSlots()
    // {
    //     CardSlot[] res = new CardSlot[3];
    //     for(int i = 0; i < 3; i++)
    //     {
    //         res[i] = TopCardSprites[i].gameObject.GetComponent<CardSlot>();
    //     }
    //     return res;
    // }

    public void SyncStampsToCardSlots()
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        NetworkArray<int> allStamps = GameManager.Instance.CardAttachedStamps;

        SpriteRenderer[] hostVisualSlots = amIHost ? BottomCardSprites : TopCardSprites;
        SpriteRenderer[] clientVisualSlots = amIHost ? TopCardSprites : BottomCardSprites;

        PopulateStampsForPlayer(GameManager.Instance.HostHand,   hostVisualSlots,   allStamps);
        PopulateStampsForPlayer(GameManager.Instance.ClientHand, clientVisualSlots, allStamps);
    }

    private void PopulateStampsForPlayer(NetworkArray<int> playerHand, SpriteRenderer[] visualSlots, NetworkArray<int> allStamps)
    {
        for(int slotIndex = 0; slotIndex < 3; slotIndex++)
        {
            int cardID = playerHand[slotIndex];
            if (cardID == -1) continue;

            CardSlot cardSlot = visualSlots[slotIndex].GetComponent<CardSlot>();
            if (cardSlot == null) continue;

            cardSlot.Stamps.Clear();

            // Joker chỉ có 1 ô stamp
            int stampCount = GameConstants.IsJokerStamp(cardID) ? 1 : 3;
            int startIndex = cardID * 3;

            for (int i = 0; i < stampCount; i++)
            {
                int stampID = allStamps[startIndex + i];
                if (stampID <= 0) continue;

                BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);
                if (stampData != null)
                {
                    stampData.isEnabled = true; // reset trước khi resolve
                    cardSlot.Stamps.Add(stampData);
                }
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
}

using System.Collections;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class TableVisualManager : Singleton<TableVisualManager>
{
    [Header("Bottom Cards")]
    public SpriteRenderer[] BottomCardSprites = new SpriteRenderer[3];
    public TextMeshPro[] BottomCardTexts = new TextMeshPro[3];

    [Header("Top Cards")]
    public SpriteRenderer[] TopCardSprites = new SpriteRenderer[3];
    public TextMeshPro[] TopCardTexts = new TextMeshPro[3];

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

    private void Start()
    {
        for(int i = 0; i < 3; i++)
        {
            BottomCardSprites[i].gameObject.SetActive(false);
            TopCardSprites[i].gameObject.SetActive(false);
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

    public void PlayDealAnimation(CardData[] cards, bool isHostCard)
    {
        bool amIHost = GameManager.Instance.Runner.IsServer;
        bool isMyCard = (amIHost && isHostCard) || (!amIHost && !isHostCard);

        SpriteRenderer[] targetSlots = isMyCard ? BottomCardSprites : TopCardSprites;
        TextMeshPro[] targetScores = isMyCard ? BottomCardTexts : TopCardTexts;

        StartCoroutine(DealCardsRoutine(cards, targetSlots, targetScores));
    }

    private IEnumerator DealCardsRoutine(CardData[] cards, SpriteRenderer[] slots, TextMeshPro[] scores)
    {
        // lưu vị trí
        Vector3[] targetPositions = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            targetPositions[i] = slots[i].transform.position;
        }

        // chia bài anim
        for (int i = 0; i < 3; i++)
        {
            slots[i].gameObject.SetActive(true);
            // set up lá bài: ở giữa, úp bài, ẩn giá trị
            slots[i].transform.position = _mainDeckTransform.position;
            slots[i].transform.rotation = Quaternion.Euler(0, 0, 90f);
            slots[i].sprite = CardBackSprite;
            scores[i].text = ""; 
            
            // ép nhỏ bài
            slots[i].transform.localScale = Vector3.zero;

            // phóng bài ra
            slots[i].transform.DOScale(_cardDealScale, _animCardSlideDuration).SetEase(Ease.OutBack);
            slots[i].transform.DORotate(new Vector3(0, 0, 0), _animCardSlideDuration).SetEase(Ease.OutBack);
            slots[i].transform.DOMove(targetPositions[i], _animCardSlideDuration).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(_animCardSlideDuration);

            // flip bài
            slots[i].transform.DOScaleX(0f, 0.15f).OnComplete(() => 
            {
                // lúc lá bài mỏng -> đổi sprite
                slots[i].sprite = cards[i].Artwork;
                scores[i].text = cards[i].BaseScore.ToString();
                // mở bài
                slots[i].transform.DOScale(_cardScale, 0.15f).SetEase(Ease.OutBack);
            });
            yield return new WaitForSeconds(0.3f); 
        }

        // chuyển sang main phase
        GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.MainPhase);
    }
}

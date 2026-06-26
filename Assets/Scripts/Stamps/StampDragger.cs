using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.VisualScripting;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class StampDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("References")]
    public GameObject stampToolPrefab;
    public int stampID = 1;
    public bool isUsed = false;

    [Header("Game Juice Settings")]
    [SerializeField] private float _pickUpScale = 1.5f;
    [SerializeField] private ParticleSystem _bloodImpactParticle;
    [SerializeField] private float _shakeAmplitude = 0.3f;
    [SerializeField] private float _shakeDuration = 0.2f;

    [Header("Stamp Settings")]
    [SerializeField] private float _stampStartPosition = 5f;
    [SerializeField] private float _stampSpeed = 10f;
    [SerializeField] private float _stampImpactScale = 1.5f;
    [SerializeField] private float _stampImpactDuration = 0.2f;

    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer _frame;
    private Vector2 _originalPosition;
    public Vector2 _originalScale = Vector2.one;
    private int _originalSortingOrder;

    private bool _isDragging = false;
    private bool _isReturning = false;

    private GameObject _stampToolInstance;

    public Vector2 OriginalScale => _originalScale;
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalPosition = transform.position;
        _originalScale = transform.localScale;
        _originalSortingOrder = _spriteRenderer.sortingOrder;

        _stampToolInstance = Instantiate(stampToolPrefab, Vector2.zero, Quaternion.identity);
        _stampToolInstance.SetActive(false); // Ẩn nó đi trước khi dùng
        isUsed = false;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(isUsed) return;
        _isDragging = true;
        _spriteRenderer.sortingOrder = 100; // Đưa lên trên cùng
        _frame.sortingOrder = 101;
        transform.DOScale(_originalScale * _pickUpScale, 0.1f).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(isUsed) return;
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPoint.z = 0f; 
        
        transform.position = Vector3.Lerp(transform.position, worldPoint, 30f * Time.deltaTime);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(isUsed) return;
        _spriteRenderer.sortingOrder = _originalSortingOrder;
        _frame.sortingOrder = _originalSortingOrder + 1;
        transform.DOScale(_originalScale, 0.1f).SetEase(Ease.OutBack);
        Vector2 dropPoint = Camera.main.ScreenToWorldPoint(eventData.position);

        Collider2D[] hits = Physics2D.OverlapPointAll(dropPoint);

        bool hasFoundCard = false;
        _isDragging = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("MyCard"))
            {
                StampOnCard(hit.gameObject);
                hasFoundCard = true;
                break; 
            }
        }

        if (!hasFoundCard)
        {
            ReturnToStart();
        }
    }

    private void ReturnToStart()
    {
        _isReturning = true; 

        transform.DOKill(); 
        transform.DOScale(_originalScale, 0.1f).SetEase(Ease.OutBack);
        transform.DOMove(_originalPosition, 0.3f).SetEase(Ease.OutBack).OnComplete(() => 
        {
            _isReturning = false; 
        });
    }

    private void StampOnCard(GameObject targetCard)
    {
        TableVisualManager.Instance.HideUnusedStamps(this.gameObject);
        int slotIndex = targetCard.GetComponent<CardSlot>().Index;

        _spriteRenderer.enabled = false;
        //_frame.enabled = false;
        _frame.gameObject.SetActive(false);
        isUsed = true;

        // Spawn stamp tool animation
        Vector3 spawnPos = targetCard.transform.position + Vector3.up * _stampStartPosition;
        _stampToolInstance.SetActive(true);
        _stampToolInstance.transform.position = spawnPos;
        _stampToolInstance.GetComponent<SpriteRenderer>().sortingOrder = 200;

        // Reset fade phòng trường hợp lượt trước bị dở
        _stampToolInstance.GetComponent<SpriteRenderer>().DOFade(1f, 0f);

        DG.Tweening.Sequence seq = DOTween.Sequence();

        seq.Append(_stampToolInstance.transform
            .DOMove(targetCard.transform.position, _stampImpactDuration)
            .SetEase(Ease.InExpo));

        seq.AppendCallback(() =>
        {
            targetCard.transform.DOPunchScale(new Vector3(0.25f, -0.2f, 0), _shakeDuration, 10, 1f);
            Camera.main.transform.DOShakePosition(_shakeDuration, _shakeAmplitude, 20, 90f);
            FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor, _shakeDuration);

            if (_bloodImpactParticle != null)
            {
                _bloodImpactParticle.transform.position = targetCard.transform.position;
                _bloodImpactParticle.gameObject.SetActive(true);
                _bloodImpactParticle.Play();
            }
        });

        seq.Append(_stampToolInstance.transform
            .DOMove(targetCard.transform.position + Vector3.up * 2f, _stampImpactDuration)
            .SetEase(Ease.OutQuad));
        seq.Join(_stampToolInstance.GetComponent<SpriteRenderer>()
            .DOFade(0, _stampImpactDuration)
            .SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            _stampToolInstance.SetActive(false);

            bool amIHost = GameManager.Instance.Runner.IsServer;
            GameManager.Instance.RPC_PlayStamp(slotIndex, stampID, amIHost);

            _frame.gameObject.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    public void ResetForNewTurn(int newStampID, Sprite newSprite, Sprite frame)
    {
        stampID = newStampID;
        isUsed = false;
        _spriteRenderer.enabled = true;
        //_frame.enabled = true;
        _frame.gameObject.SetActive(true);
        _spriteRenderer.sprite = newSprite;
        _frame.sprite = frame;
        _spriteRenderer.DOFade(1f, 0f);
        _frame.DOFade(1f, 0f);

        transform.DOKill();
        transform.position = _originalPosition;
        transform.localScale = _originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isUsed) return;
        if(_isDragging || _isReturning) return;
        BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);

        if (stampData != null)
        {
            TooltipManager.Instance.ShowTooltip(stampData.stampName, stampData.stampEffect);
            
            transform.DOKill();
            transform.DOScale(_originalScale * 1.2f, 0.15f).SetEase(Ease.OutBack);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(isUsed) return;
        if(_isDragging || _isReturning) return;

        TooltipManager.Instance.HideTooltip();
        transform.DOKill();
        transform.DOScale(_originalScale, 0.15f).SetEase(Ease.InQuad);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if(isUsed) return;
        if (_isReturning) return;
        _isDragging = true;
        TooltipManager.Instance.HideTooltip();
    }
}
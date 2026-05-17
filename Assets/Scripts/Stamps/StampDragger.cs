using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc phải có
using DG.Tweening;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class StampDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public GameObject stampToolPrefab;
    public int stampID = 1;
    public bool isUsed = false;

    [Header("Game Juice Settings")]
    [SerializeField] private float _pickUpScale = 1.2f;
    [SerializeField] private ParticleSystem _bloodImpactParticle;
    [SerializeField] private float _shakeAmplitude = 0.2f;
    [SerializeField] private float _shakeDuration = 0.2f;

    [Header("Stamp Settings")]
    [SerializeField] private float _stampStartPosition = 5f;
    [SerializeField] private float _stampSpeed = 10f;
    [SerializeField] private float _stampImpactScale = 1.5f;
    [SerializeField] private float _stampImpactDuration = 0.2f;

    private SpriteRenderer _spriteRenderer;
    private Vector2 _originalPosition;
    public Vector2 _originalScale = Vector2.one;
    private int _originalSortingOrder;

    private GameObject _stampToolInstance;

    public Vector2 OriginalScale => _originalScale;
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalPosition = transform.position;
        //_originalScale = transform.localScale;
        _originalSortingOrder = _spriteRenderer.sortingOrder;

        _stampToolInstance = Instantiate(stampToolPrefab, Vector2.zero, Quaternion.identity);
        _stampToolInstance.SetActive(false); // Ẩn nó đi trước khi dùng
    }

    // 1. KHI VỪA NẮM CHUỘT KÉO ĐI
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(isUsed) return;
        _spriteRenderer.sortingOrder = 100; // Đưa lên trên cùng
        transform.DOScale(_originalScale * _pickUpScale, 0.1f).SetEase(Ease.OutBack);
    }

    // 2. KHI ĐANG RÊ CHUỘT
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
        transform.DOScale(_originalScale, 0.1f).SetEase(Ease.OutBack);
        Vector2 dropPoint = Camera.main.ScreenToWorldPoint(eventData.position);

        Collider2D[] hits = Physics2D.OverlapPointAll(dropPoint);

        bool hasFoundCard = false;
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
        transform.DOScale(_originalScale, 0.1f).SetEase(Ease.OutBack);
        transform.DOMove(_originalPosition, 0.3f).SetEase(Ease.OutBack);
    }

    private void StampOnCard(GameObject targetCard)
    {
        TableVisualManager.Instance.HideUnusedStamps(this.gameObject);
        int slotIndex = targetCard.GetComponent<CardSlot>().Index;

        _spriteRenderer.enabled = false;

        // Spawn stamp tool animation
        Vector3 spawnPos = targetCard.transform.position + Vector3.up * _stampStartPosition;
        _stampToolInstance.SetActive(true);
        _stampToolInstance.transform.position = spawnPos;
        _stampToolInstance.GetComponent<SpriteRenderer>().sortingOrder = 200;

        // Reset fade phòng trường hợp lượt trước bị dở
        _stampToolInstance.GetComponent<SpriteRenderer>().DOFade(1f, 0f);

        Sequence seq = DOTween.Sequence();

        seq.Append(_stampToolInstance.transform
            .DOMove(targetCard.transform.position, _stampImpactDuration)
            .SetEase(Ease.InExpo));

        seq.AppendCallback(() =>
        {
            targetCard.transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0), _shakeDuration, 10, 1f);
            Camera.main.transform.DOShakePosition(_shakeDuration, _shakeAmplitude, 20, 90f);

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
            isUsed = true;

            bool amIHost = GameManager.Instance.Runner.IsServer;
            GameManager.Instance.RPC_PlayStamp(slotIndex, stampID, amIHost);

            gameObject.SetActive(false);
        });
    }
}
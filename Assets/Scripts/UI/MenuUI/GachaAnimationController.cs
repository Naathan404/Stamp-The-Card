using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using TMPro;

public class GachaAnimationController : MonoBehaviour, IPointerClickHandler
{
    [Header("Gacha UI Elements")]
    [SerializeField] private Transform normalBundleRect;
    [SerializeField] private Transform mediumBundleRect;
    [SerializeField] private Transform largeBundleRect;
    private Vector3 normalOriginPos;
    private Vector3 mediumOriginPos;
    private Vector3 largeOriginPos;
    private Sequence _activeSeq;
    public bool IsGachaRunning = false;

    [SerializeField] private CanvasGroup flashWhiteGroup;
    [SerializeField] private RectTransform backgroundSummon;
    [SerializeField] private TextMeshProUGUI soulQuantityCost;

    [Header("Stamp result UI")]
    public Image stampResultSprite;
    [SerializeField] private CanvasGroup stampDescriptionRect;  
    public TextMeshProUGUI stampResultName;
    public TextMeshProUGUI stampResultRarity;
    public TextMeshProUGUI stampResultEffect;
    public GameObject newTagObj;

    [Header("Animation Settings")]
    [SerializeField] private float shakeDuration = 1.2f;
    [SerializeField] private float popupDuration = 0.5f;

    public static GachaAnimationController Instance;
    private bool CanClickToReset = false;

    private ShopUIController _shopUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (_shopUI == null)
            _shopUI = FindAnyObjectByType<ShopUIController>();

        stampDescriptionRect.alpha = 0f;
        normalOriginPos = normalBundleRect.localPosition;
        mediumOriginPos = mediumBundleRect.localPosition;
        largeOriginPos = largeBundleRect.localPosition;
    }

    public void Normal_BundlePlayGachaAnimation(bool isNew)
    {
        soulQuantityCost.text = "-49 SOULS";
        PlayGachaAnimation(normalBundleRect, isNew);
    }

    public void Medium_BundlePlayGachaAnimation(bool isNew)
    {
        soulQuantityCost.text = "-79 SOULS";
        PlayGachaAnimation(mediumBundleRect, isNew);
    }

    public void Large_BundlePlayGachaAnimation(bool isNew)
    {
        soulQuantityCost.text = "-129 SOULS";
        PlayGachaAnimation(largeBundleRect, isNew);
    }

    public void Normal_BundleInsufficientSoulsPlayGachaAnimation()
    {
        InsufficientSoulsPlayGachaAnimation(normalBundleRect);
    }

    public void Medium_BundleInsufficientSoulsPlayGachaAnimation()
    {
        InsufficientSoulsPlayGachaAnimation(mediumBundleRect);
    }

    public void Large_BundleInsufficientSoulsPlayGachaAnimation()
    {
        InsufficientSoulsPlayGachaAnimation(largeBundleRect);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanClickToReset)
        {
            SetUpOriginState();
            CanClickToReset = false;
            IsGachaRunning = false;
        }
    }

    private void PlayGachaAnimation(Transform bundleRect, bool isNew)
    {
        IsGachaRunning = true;
        CanClickToReset = false;
        SetUpOriginState();

        if (FilterManager.Instance != null)
            FilterManager.Instance.SetFocusMode(true, 0.5f);

        soulQuantityCost.rectTransform.position = Camera.main.WorldToScreenPoint(bundleRect.position);
        soulQuantityCost.gameObject.SetActive(true);
        soulQuantityCost.alpha = 1f;

        Sequence _activeSeq = DOTween.Sequence();

        _activeSeq.Append(bundleRect.DOShakeRotation(shakeDuration, new Vector3(0, 0, 15f), 10, 90f, false));
        _activeSeq.Join(bundleRect.DOShakeScale(shakeDuration, 0.15f, 10, 90f, false));
        _activeSeq.Join(soulQuantityCost.rectTransform.DOLocalMoveY(soulQuantityCost.rectTransform.localPosition.y + 500f, shakeDuration).SetEase(Ease.OutQuad));

        _activeSeq.AppendCallback(() => {
            if (FilterManager.Instance != null)
                FilterManager.Instance.FlashScreen(FilterManager.Instance.FlashColor, 0.5f);
        });
        _activeSeq.Append(flashWhiteGroup.DOFade(1f, 0.15f));
        _activeSeq.Join(Camera.main.transform.DOShakePosition(0.3f, 0.5f, 20, 90f, false, true));

        _activeSeq.AppendCallback(() =>
        {
            bundleRect.gameObject.SetActive(false);
            soulQuantityCost.gameObject.SetActive(false); 
            stampResultSprite.gameObject.SetActive(true);
            stampResultSprite.transform.localScale = Vector3.zero; 
            backgroundSummon.gameObject.SetActive(true);

        });

        _activeSeq.Append(flashWhiteGroup.DOFade(0f, 0.3f)); 

        _activeSeq.Append(stampResultSprite.transform.DOScale(Vector3.one, popupDuration).SetEase(Ease.OutBack));
        _activeSeq.Join(stampResultSprite.transform.DORotate(new Vector3(0, 360, 0), popupDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

        if (isNew && newTagObj != null)
        {
            _activeSeq.AppendCallback(() => newTagObj.SetActive(true));

            _activeSeq.Append(newTagObj.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutElastic));

            _activeSeq.Join(newTagObj.transform.DOShakeRotation(0.4f, new Vector3(0, 0, 15f), 10, 90f));
        }

        _activeSeq.AppendCallback(() => {
            if (FilterManager.Instance != null)
                FilterManager.Instance.FlashVignette(FilterManager.Instance.FlashColor, 0.8f, 0.8f);
            
        });

        _activeSeq.Append(stampDescriptionRect.DOFade(1f, 0.5f).SetEase(Ease.InCubic));
        _activeSeq.Join(Camera.main.transform.DOShakePosition(0.15f, 0.5f, 20, 90f, false, true));

        _activeSeq.OnComplete(() =>
        {
            flashWhiteGroup.gameObject.SetActive(false);
            CanClickToReset = true;
            if (_shopUI != null) _shopUI.UpdateShopUI();
        });
    }

    private void InsufficientSoulsPlayGachaAnimation(Transform bundleRect)
    {
        IsGachaRunning = true;
        soulQuantityCost.text = "INSUFFICIENT SOULS!";
        CanClickToReset = false;
        
        SetUpOriginState(); 
        _activeSeq = DOTween.Sequence(); 
        FilterManager.Instance.FlashVignette(FilterManager.Instance.HazardColor, 0.6f, 0.8f);

        soulQuantityCost.rectTransform.position = Camera.main.WorldToScreenPoint(bundleRect.position);

        _activeSeq.Append(bundleRect.DOShakePosition(0.5f, new Vector3(0.5f, 0, 0), 20, 90f, false, true));

        SpriteRenderer bundleSprite = bundleRect.GetComponent<SpriteRenderer>();
        if (bundleSprite != null)
        {
            _activeSeq.Join(bundleSprite.DOColor(Color.red, 0.15f).SetLoops(4, LoopType.Yoyo));
        }

        _activeSeq.AppendCallback(() =>
        {
            IsGachaRunning = false;
            soulQuantityCost.gameObject.SetActive(true);
        });

        _activeSeq.Append(soulQuantityCost.rectTransform.DOLocalMoveY(soulQuantityCost.rectTransform.localPosition.y + 100f, 1f).SetEase(Ease.OutQuad));
        _activeSeq.Join(soulQuantityCost.DOFade(0f, 1f));
    }

    private void SetUpOriginState()
    {
        // IsGachaRunning = false;
        if (_activeSeq != null && _activeSeq.IsActive())
        {
            _activeSeq.Kill();
        }
        DOTween.Kill(soulQuantityCost);
        DOTween.Kill(soulQuantityCost.rectTransform);

        soulQuantityCost.gameObject.SetActive(false);
        soulQuantityCost.alpha = 1f;

        normalBundleRect.gameObject.SetActive(true);
        ResetBundleState(normalBundleRect, normalOriginPos);

        mediumBundleRect.gameObject.SetActive(true);
        ResetBundleState(mediumBundleRect, mediumOriginPos);

        largeBundleRect.gameObject.SetActive(true);
        ResetBundleState(largeBundleRect, largeOriginPos);

        stampResultSprite.gameObject.SetActive(false);
        stampDescriptionRect.alpha = 0f;

        if (newTagObj != null) 
        {
            newTagObj.SetActive(false);
            newTagObj.transform.localScale = Vector3.zero; 
            DOTween.Kill(newTagObj.transform);
        }

        flashWhiteGroup.alpha = 0f;
        flashWhiteGroup.gameObject.SetActive(true);
        backgroundSummon.gameObject.SetActive(false);

        if (FilterManager.Instance != null)
            FilterManager.Instance.SetFocusMode(false, 0.5f);
    }

    private void ResetBundleState(Transform bundle, Vector3 originPos)
    {
        DOTween.Kill(bundle); 
        bundle.localPosition = originPos;
        bundle.localScale = Vector3.one;
        bundle.localRotation = Quaternion.identity;

        SpriteRenderer sr = bundle.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            DOTween.Kill(sr); 
            sr.color = Color.white; 
        }
    }
}
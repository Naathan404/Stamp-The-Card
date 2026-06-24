using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;
using TMPro;

public class GachaAnimationController : MonoBehaviour, IPointerClickHandler
{
    [Header("Gacha UI Elements")]
    [SerializeField] private RectTransform normalBundleRect;
    [SerializeField] private RectTransform mediumBundleRect;
    [SerializeField] private RectTransform largeBundleRect;

    [SerializeField] private Outline normalBundleOutline;
    [SerializeField] private Outline mediumBundleOutline;
    [SerializeField] private Outline largeBundleOutline;

    [SerializeField] private RectTransform stampResultRect;  
    [SerializeField] private CanvasGroup flashWhiteGroup;
    [SerializeField] private RectTransform backgroundSummon;
    [SerializeField] private TextMeshProUGUI soulQuantityCost;

    [Header("Stamp result UI")]
    public Image stampResultSprite;
    public TextMeshProUGUI stampResultName;
    public TextMeshProUGUI stampResultRarity;
    public TextMeshProUGUI stampResultEffect;

    [Header("Animation Settings")]
    [SerializeField] private float shakeDuration = 1.2f;
    [SerializeField] private float popupDuration = 0.5f;

    public static GachaAnimationController Instance;
    private bool CanClickToReset = false;
    private Color startColorOutline;

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

        startColorOutline = normalBundleOutline.effectColor;
        startColorOutline.a = 0f;
    }

    public void Normal_BundlePlayGachaAnimation()
    {
        soulQuantityCost.text = "-49 SOULS";

        PlayGachaAnimation(normalBundleRect);
    }

    public void Medium_BundlePlayGachaAnimation()
    {
        soulQuantityCost.text = "-79 SOULS";

        PlayGachaAnimation(mediumBundleRect);
    }

    public void Large_BundlePlayGachaAnimation()
    {
        soulQuantityCost.text = "-129 SOULS";

        PlayGachaAnimation(largeBundleRect);
    }

    public void Normal_BundleInsufficientSoulsPlayGachaAnimation()
    {
        InsufficientSoulsPlayGachaAnimation(normalBundleRect, normalBundleOutline);
    }

    public void Medium_BundleInsufficientSoulsPlayGachaAnimation()
    {
        InsufficientSoulsPlayGachaAnimation(mediumBundleRect, mediumBundleOutline);
    }

    public void Large_BundleInsufficientSoulsPlayGachaAnimation()
    {
        InsufficientSoulsPlayGachaAnimation(largeBundleRect, largeBundleOutline);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanClickToReset)
        {
            SetUpOriginState();
            CanClickToReset = false;
        }
    }

    private void PlayGachaAnimation(RectTransform bundleRect)
    {

        // Setup trang thai ban dau
        CanClickToReset = false;
        SetUpOriginState();

        // Tao sequence de ket noi hieu ung
        Sequence gachaSeq = DOTween.Sequence();

        // --- RUNG LAC BUNDLE ---
        // Lắc xoay (Z axis) và lắc scale cùng lúc
        gachaSeq.Append(bundleRect.DOShakeRotation(shakeDuration, new Vector3(0, 0, 15f), 10, 90f, false));
        gachaSeq.Join(bundleRect.DOShakeScale(shakeDuration, 0.15f, 10, 90f, false));

        // --- BƯỚC 2: CHỚP SÁNG ---
        gachaSeq.Append(flashWhiteGroup.DOFade(1f, 0.15f));

        // Callback: Thực hiện logic tráo đổi UI khi màn hình đang trắng xóa
        gachaSeq.AppendCallback(() =>
        {
            bundleRect.gameObject.SetActive(false);
            stampResultRect.gameObject.SetActive(true);
            backgroundSummon.gameObject.SetActive(true);
            soulQuantityCost.gameObject.SetActive(true);
        });

        gachaSeq.Append(flashWhiteGroup.DOFade(0f, 0.3f)); // Mờ dần đi

        // --- BƯỚC 3: HIỂN THỊ TEM ---
        gachaSeq.Join(soulQuantityCost.DOFade(0f, 1f));
        gachaSeq.Join(stampResultRect.DOScale(Vector3.one, popupDuration).SetEase(Ease.OutBack));
        gachaSeq.Join(stampResultRect.DORotate(new Vector3(0, 360, 0), popupDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

        // --- HOÀN THÀNH ---
        gachaSeq.OnComplete(() =>
        {
            flashWhiteGroup.gameObject.SetActive(false);
            CanClickToReset = true;

            _shopUI.UpdateShopUI();
        });
    }

    private void InsufficientSoulsPlayGachaAnimation(RectTransform bundleRect, Outline bundleOutline)
    {
        soulQuantityCost.text = "INSUFFICIENT SOULS!";
        CanClickToReset = false;
        SetUpOriginState();

        DOTween.Kill(bundleOutline);
        DOTween.Kill(bundleRect);

        Sequence gachaSeq = DOTween.Sequence();

        Color solidStartColorOutline = startColorOutline;
        solidStartColorOutline.a = 1f;
        gachaSeq.Append(
            DOTween.To(
            () => bundleOutline.effectColor,             // Lấy giá trị hiện tại
            x => bundleOutline.effectColor = x,          // Gán giá trị mới trong quá trình chạy
            solidStartColorOutline,                      // Giá trị đích (màu đỏ đậm)
            0.15f                                        // Thời gian
            )
            .SetLoops(4, LoopType.Yoyo)
            .OnComplete(() =>
            {
                // Trả về tàng hình khi kết thúc
                bundleOutline.effectColor = startColorOutline;
            })
        );

        gachaSeq.AppendCallback(() =>
        {
            soulQuantityCost.gameObject.SetActive(true);
        });

        gachaSeq.Append(soulQuantityCost.DOFade(0f, 1f));
    }

    private void SetUpOriginState()
    {
        soulQuantityCost.gameObject.SetActive(false);
        soulQuantityCost.alpha = 1f;

        normalBundleRect.gameObject.SetActive(true);
        normalBundleRect.localScale = Vector3.one;
        normalBundleRect.localRotation = Quaternion.identity;
        normalBundleOutline.effectColor = startColorOutline;

        mediumBundleRect.gameObject.SetActive(true);
        mediumBundleRect.localScale = Vector3.one;
        mediumBundleRect.localRotation = Quaternion.identity;
        mediumBundleOutline.effectColor = startColorOutline;

        largeBundleRect.gameObject.SetActive(true);
        largeBundleRect.localScale = Vector3.one;
        largeBundleRect.localRotation = Quaternion.identity;
        largeBundleOutline.effectColor = startColorOutline;

        stampResultRect.gameObject.SetActive(false);
        stampResultRect.localScale = Vector3.zero;

        flashWhiteGroup.alpha = 0f;
        flashWhiteGroup.gameObject.SetActive(true);

        backgroundSummon.gameObject.SetActive(false);
    }
}
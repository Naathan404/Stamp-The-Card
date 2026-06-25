using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

// Bắt buộc phải có Collider2D để Physics 2D Raycaster bắt được
[RequireComponent(typeof(BoxCollider2D))]
public class BundleClickable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum BundleType { Normal, Medium, Large }
    
    public BundleType bundleType;

    private ShopUIController _shopUI;
    private Vector3 _originalScale;


    private void Start()
    {
        _shopUI = FindAnyObjectByType<ShopUIController>();
        _originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_shopUI == null) 
        {
            Debug.LogError("[BundleClickable] Không tìm thấy ShopUIController!");
            return;
        }

        switch (bundleType)
        {
            case BundleType.Normal:
                _shopUI.OpenNormalStampPack();
                break;
            case BundleType.Medium:
                _shopUI.OpenMediumStampPack();
                break;
            case BundleType.Large:
                _shopUI.OpenLargeStampPack();
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GachaAnimationController.Instance != null && GachaAnimationController.Instance.IsGachaRunning) 
            return;

        transform.DOScale(_originalScale * 1.05f, 0.2f).SetEase(Ease.OutQuad);
        if (_shopUI != null) _shopUI.ShowTooltip(bundleType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_originalScale, 0.2f).SetEase(Ease.OutQuad);
        if (_shopUI != null) _shopUI.HideTooltip();
    }
}
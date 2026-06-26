using DG.Tweening;
using UnityEngine;
using UnityEngine.UI; // Hoặc dùng SpriteRenderer nếu bài là Object 2D

public class CardStatusUI : MonoBehaviour
{
    [Header("Icon Objects")]
    [SerializeField] private GameObject _silencedIcon;      // Icon khóa Stamp 
    [SerializeField] private GameObject _burnedIcon;        // Icon ngọn lửa 
    [SerializeField] private GameObject _toughIcon;         // Icon khiên 
    [SerializeField] private GameObject _reverseIcon;       // Icon mũi tên đảo chiều 
    [SerializeField] private GameObject _peaceAmuletIcon;   // Icon bùa hộ mệnh 
    [SerializeField] private GameObject _immuneIcon;
    [SerializeField] private Vector3 _startOffset = new Vector3(-0.8f, 1.2f, 0f);

    [SerializeField] private float _spacing = 0.4f;
    [SerializeField] private float _animDuration = 0.3f;

    private Vector2 _originalScale;

    private void Start()
    {
        _originalScale = _silencedIcon.transform.localScale;
    }

    /// <summary>
    /// Hàm cập nhật trạng thái icon kèm hiệu ứng DOTween mượt mà
    /// </summary>
    public void RefreshStatusIcons(CardSlot slot)
    {
        if (slot == null || slot.Data == null)
        {
            ToggleAllIconsWithAnim(false);
            return;
        }

        bool[] conditions = {
            slot.StampsDisabled,
            slot.IsIgnored,
            slot.IsKingOfToughness,
            slot.IsReverseBalance,
            slot.HasPeaceAmulet,
            slot.IsImmuneLowerScore
        };

        GameObject[] icons = { _silencedIcon, _burnedIcon, _toughIcon, _reverseIcon, _peaceAmuletIcon, _immuneIcon };
        int activeCount = 0;

        for (int i = 0; i < icons.Length; i++)
        {
            GameObject icon = icons[i];
            if (icon == null) continue;

            bool shouldBeActive = conditions[i];

            if (shouldBeActive)
            {
                float localX = _startOffset.x + (activeCount * _spacing);
                float localY = _startOffset.y;
                Vector3 targetLocalPos = new Vector3(localX, localY, 0f);
                if (!icon.activeSelf)
                {
                    icon.transform.localPosition = targetLocalPos;
                    icon.SetActive(true);

                    icon.transform.localScale = Vector3.zero;
                    icon.transform.DOKill();
                    icon.transform.DOScale(_originalScale, _animDuration).SetEase(Ease.OutBack);

                    icon.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
                    icon.transform.DOLocalRotate(Vector3.zero, _animDuration).SetEase(Ease.OutQuad);
                }
                else
                {
                    icon.transform.DOKill();
                    icon.transform.DOLocalMove(targetLocalPos, 0.25f).SetEase(Ease.OutQuad);
                    icon.transform.DOScale(_originalScale, 0.2f); 
                }

                activeCount++;
            }
            else
            {
                if (icon.activeSelf)
                {
                    icon.transform.DOKill();
                    icon.transform.DOScale(Vector3.zero, 0.15f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => icon.SetActive(false));
                }
            }
        }
    }

    /// <summary>
    /// Hàm tắt sạch icon có kèm hiệu ứng thu nhỏ
    /// </summary>
    private void ToggleAllIconsWithAnim(bool active)
    {
        GameObject[] icons = { _silencedIcon, _burnedIcon, _toughIcon, _reverseIcon, _peaceAmuletIcon };
        foreach (var icon in icons)
        {
            if (icon == null) continue;
            if (active)
            {
                icon.SetActive(true);
                icon.transform.localScale = _originalScale;
            }
            else
            {
                if (icon.activeSelf)
                {
                    icon.transform.DOKill();
                    icon.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => icon.SetActive(false));
                }
            }
        }
    }

    /// <summary>
    /// Hàm tắt nóng toàn bộ icon ngay lập tức 
    /// </summary>
    public void ForceClearIcons()
    {
        GameObject[] icons = { _silencedIcon, _burnedIcon, _toughIcon, _reverseIcon, _peaceAmuletIcon, _immuneIcon };
        foreach (var icon in icons)
        {
            if (icon != null && icon.activeSelf)
            {
                icon.transform.DOKill();
                icon.SetActive(false);
            }
        }
    }
}
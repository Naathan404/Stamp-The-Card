using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StampTooltipManager : Singleton<StampTooltipManager>
{
    [Header("Tooltip")]
    [SerializeField] private GameObject _tooltip;

    [Header("UI Elements")]
    [SerializeField] private Image _stampImage;
    [SerializeField] private TextMeshProUGUI _stampName;
    [SerializeField] private TextMeshProUGUI _stampRarity;
    [SerializeField] private TextMeshProUGUI _stampEffect;

    public void SetUpTooltipUI(Sprite sprite, string name, string rarity, string effect)
    {
        _stampImage.sprite = sprite;
        _stampName.text = name;
        _stampRarity.text = rarity;
        _stampEffect.text = effect;
    }
    public void ShowToolTip()
    {
        _tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        _tooltip.gameObject.SetActive(false);
    }

    public Transform GetTooltipTransform()
    { 
        return _tooltip.gameObject.transform;
    }
}

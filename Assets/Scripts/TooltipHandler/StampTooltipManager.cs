using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StampTooltipManager : Singleton<StampTooltipManager>
{
    [Header("Tooltip")]
    [SerializeField] private GameObject _tooltip;

    [Header("UI Elements")]
    [SerializeField] private Image _stampImage;
    [SerializeField] private Image _frameImage;
    [SerializeField] private TextMeshProUGUI _stampName;
    [SerializeField] private TextMeshProUGUI _stampRarity;
    [SerializeField] private TextMeshProUGUI _stampEffect;

    public void SetUpTooltipUI(Sprite sprite, Sprite frame, string name, StampRank rarity, string effect)
    {
        _stampImage.sprite = sprite;
        _frameImage.sprite = frame;
        _stampName.text = name;
        _stampRarity.text = rarity.ToString();
        _stampEffect.text = effect;

        switch(rarity)
        {
            case StampRank.RARE:
                _stampRarity.color = GameConstants.STAMP_RARE_COLOR;
                break;
            case StampRank.EPIC:
                _stampRarity.color = GameConstants.STAMP_EPIC_COLOR;
                break;
            case StampRank.LEGENDARY:
                _stampRarity.color = GameConstants.STAMP_LEGENDARY_COLOR;
                break;
            default:
                _stampRarity.color = GameConstants.STAMP_COMMON_COLOR;
                break;
        }
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

using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FilterManager : Singleton<FilterManager>
{
    [Header("References")]
    [SerializeField] private Volume _globalVolume;
    
    private ColorAdjustments _colorAdjustments;

    private void Start()
    {
        if(_globalVolume != null && _globalVolume.profile.TryGet(out _colorAdjustments))
        {
            _colorAdjustments.colorFilter.value = Color.white;
        }
        else
        {
            Debug.LogError("[FilterManager] Không tìm thấy _global volum");
        }
    }


    /// <summary>
    /// Hàm nhấp nháy màn hình
    /// </summary>
    /// <param name="targetColor": Màu nhấp nháy></param>
    /// <param name="flashDuration": Thời gian nhấp nháy></param>
    public void FlashScreen(Color targetColor, float flashDuration = 0.5f)
    {
        if(_colorAdjustments == null) return;

        DOTween.Kill(_colorAdjustments);

        _colorAdjustments.colorFilter.value = targetColor;

        DOTween.To(
            () => _colorAdjustments.colorFilter.value, 
            x => _colorAdjustments.colorFilter.value = x, 
            Color.white, 
            flashDuration
        )
        .SetEase(Ease.OutQuad)
        .SetTarget(_colorAdjustments);
    }
}

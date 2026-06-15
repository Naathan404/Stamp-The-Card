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
            _colorAdjustments.saturation.value = 0f;
            _colorAdjustments.contrast.value = 0f;
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

    public void SetDramaticFilter(bool isBlackAndWhite = true)
    {
        if(_colorAdjustments == null) return;

        _colorAdjustments.saturation.value = 0f;
        _colorAdjustments.contrast.value = 0f;
        DOTween.Kill(_colorAdjustments);
        
        float targetSat = isBlackAndWhite ? -100f : 0f;
        float targetContrast = isBlackAndWhite ? 20f : 0f;

        DOTween.To(() => _colorAdjustments.saturation.value, x => _colorAdjustments.saturation.value = x, targetSat, 1f);

        DOTween.To(() => _colorAdjustments.contrast.value, x => _colorAdjustments.contrast.value = x, targetContrast, 1f);
    }
}

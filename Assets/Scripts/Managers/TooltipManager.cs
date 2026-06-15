using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

public class TooltipManager : Singleton<TooltipManager>
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private RectTransform _rectTransform;

    private void Start()
    {
        // Ẩn tooltip khi mới vào game
        _canvasGroup.alpha = 0f;
        _canvasGroup.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_canvasGroup.gameObject.activeSelf)
        {
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                _rectTransform.position = mousePos + new Vector2(-200f, 100f); 
            }
        }
    }

    public void ShowTooltip(string stampName, string description)
    {
        _nameText.text = stampName;
        _descText.text = description;

        _canvasGroup.gameObject.SetActive(true);
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void HideTooltip()
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, 0.2f).OnComplete(() => 
        {
            _canvasGroup.gameObject.SetActive(false);
        });
    }
}
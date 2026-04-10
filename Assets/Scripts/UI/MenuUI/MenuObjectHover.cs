using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MenuObjectHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleAmount = 1.1f;
    [SerializeField] private float timeScaleChange = 0.3f;

    private Vector3 orginalScale;

    void Start()
    {
        orginalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(orginalScale * scaleAmount, timeScaleChange).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(orginalScale, timeScaleChange).SetEase(Ease.OutBack);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}

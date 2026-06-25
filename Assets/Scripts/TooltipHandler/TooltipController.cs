using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IBeginDragHandler
{
    private StampSlotUI _stampSlotUI;

    private void Start()
    {

        if (_stampSlotUI == null)
        {
            _stampSlotUI = GetComponent<StampSlotUI>();
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        //Neu dang drag item thi khong hien thi tooltip
        if (eventData.pointerDrag != null) return;

        //Thiet lap tooltip
        if (_stampSlotUI != null)
        {
            Sprite sprite = _stampSlotUI.stampImage.sprite;
            Sprite frame = _stampSlotUI.frameImage.sprite;
            BaseStampData data = _stampSlotUI.stampInstance.data;
            string name = data.stampName;
            StampRank rarity = data.stampRank;
            string effect = data.stampEffect;

            StampTooltipManager.Instance.SetUpTooltipUI(sprite, frame, name, rarity, effect);
        }

        //Hien thi tooltip
        StampTooltipManager.Instance.GetTooltipTransform().position = eventData.position + new Vector2(150, -100);
        StampTooltipManager.Instance.ShowToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StampTooltipManager.Instance.HideTooltip();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        StampTooltipManager.Instance.GetTooltipTransform().position = eventData.position + new Vector2(150, -100);
    }


    //Xu ly tooltip khi keo tha
    public void OnBeginDrag(PointerEventData eventData)
    {
        StampTooltipManager.Instance.HideTooltip();
    }
}

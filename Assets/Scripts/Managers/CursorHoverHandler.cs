using UnityEngine;
using UnityEngine.EventSystems; // Cần cái này để bắt sự kiện chuột

public class CursorHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector2 _hotSpot = new Vector2(10, 0);

    public enum CursorHoverType
    {
        HAND,
        ATTACK,
    }

    public CursorHoverType CursorType = CursorHoverType.HAND;

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch(CursorType)
        {
            case CursorHoverType.HAND:
                Cursor.SetCursor(CursorManager.Instance.HandCursorTexture, _hotSpot, CursorMode.Auto);
                break;
            case CursorHoverType.ATTACK:
                Cursor.SetCursor(CursorManager.Instance.AttackCursorTexture, _hotSpot, CursorMode.Auto);
                break;
            default:
                Cursor.SetCursor(CursorManager.Instance.DefaultCursor, Vector2.zero, CursorMode.Auto);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(CursorManager.Instance.DefaultCursor, Vector2.zero, CursorMode.Auto);
    }
}
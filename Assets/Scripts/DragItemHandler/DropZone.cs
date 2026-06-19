using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private int _maxSlot;

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount >= _maxSlot)
        {
            Debug.Log("Khong du slot de them stamp");
            return;
        }

        GameObject dropItem = eventData.pointerDrag;

        if (dropItem != null)
        {
            DragItem dragItem = dropItem.GetComponent<DragItem>();

            if (dragItem != null)
            {
                dragItem.originalParent = this.transform;

                dropItem.transform.SetParent(this.transform, false);
            }
        }
    }
}

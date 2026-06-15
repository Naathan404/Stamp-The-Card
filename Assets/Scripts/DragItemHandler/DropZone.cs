using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount >= 9)
        {
            Debug.Log("Da chon du 9 stamp!");
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

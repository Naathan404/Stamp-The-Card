using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private int _maxSlot;
    [SerializeField] private bool _isInventory;

    private SelectedStampZoneController _selectedStampZoneController;
    private InventoryUIController _inventoryUIController;

    private void Awake()
    {
        if (_selectedStampZoneController == null)
        {
            _selectedStampZoneController = FindAnyObjectByType<SelectedStampZoneController>();
        }

        if (_inventoryUIController == null)
        {
            _inventoryUIController = FindAnyObjectByType<InventoryUIController>();
        }
    }

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

                //Luu selected item vao local player data
                _selectedStampZoneController.SaveSelectedStampsToLocal();

                //Sap xep lai inventory panel
                if (_isInventory && _inventoryUIController != null)
                {
                    _inventoryUIController.SortInventoryPanelUI();
                }
            }
        }
    }
}

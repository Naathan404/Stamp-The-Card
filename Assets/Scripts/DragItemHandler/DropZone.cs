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
        _selectedStampZoneController = FindAnyObjectByType<SelectedStampZoneController>();
        _inventoryUIController = FindAnyObjectByType<InventoryUIController>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropItem = eventData.pointerDrag;
        if (dropItem != null)
        {
            DragItem dragItem = dropItem.GetComponent<DragItem>();
            StampSlotUI stampSlot = dropItem.GetComponent<StampSlotUI>();

            if (dragItem != null && stampSlot != null)
            {
                int currentStamps = GetComponentsInChildren<StampSlotUI>().Length;
                if (dragItem.originalParent != this.transform && currentStamps >= _maxSlot)
                {
                    Debug.Log("Không đủ slot để thả stamp!");
                    return; 
                }

                dragItem.originalParent = this.transform;
                dropItem.transform.SetParent(this.transform, false);

                // Dọn dẹp kho đồ
                if (_inventoryUIController != null)
                    _inventoryUIController.SortInventoryPanelUI();

                // Lưu Balo
                if (_selectedStampZoneController != null)
                    _selectedStampZoneController.SaveSelectedStampsToLocal();
            }
        }
    }
}
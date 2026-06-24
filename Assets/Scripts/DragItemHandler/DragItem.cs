using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [HideInInspector] public Transform originalParent;
    private CanvasGroup _canvasGroup;

    private GameObject copyDragItem;
    private int originalInventoryIndex;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        { 
            _canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Luu lai vi tri ban dau
        originalParent = transform.parent;
        originalInventoryIndex = transform.GetSiblingIndex();

        //Tao ban sao cua drag item
        copyDragItem = Instantiate(this.gameObject, originalParent);
        copyDragItem.transform.SetSiblingIndex(originalInventoryIndex);             //ep copy drag item nam dung vi tri cua drag item
        DragItem copyScript = copyDragItem.GetComponent<DragItem>();
        if (copyScript != null)
        {
            Destroy(copyScript);
        }
        CanvasGroup copyCanvasGroup = copyDragItem.GetComponent<CanvasGroup>();
        copyCanvasGroup.alpha = 0.5f;

        //Vo hieu hoa raycast
        _canvasGroup.blocksRaycasts = false;


        //Lay object ra khoi parent
        transform.SetParent(transform.root, false);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Xoa ban sao cua drag item
        Destroy(copyDragItem);

        //Tra trang thai ve ban dau
        _canvasGroup.blocksRaycasts = true;

        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalInventoryIndex);
        }
    }
}

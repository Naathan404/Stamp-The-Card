using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [HideInInspector] public Transform originalParent;
    private CanvasGroup _canvasGroup;

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
        //Vo hieu hoa raycast
        _canvasGroup.blocksRaycasts = false;

        originalParent = transform.parent;

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
        //Bat lai raycast
        _canvasGroup.blocksRaycasts = true;

        if (transform.parent == transform.root)
            transform.SetParent(originalParent, false);
    }
}

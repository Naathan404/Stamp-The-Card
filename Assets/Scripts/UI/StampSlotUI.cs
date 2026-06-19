using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StampSlotUI : MonoBehaviour
{
    [SerializeField] private Image _stampImage;
    public StampInstance stampInstance;

    public void SetUpStampSlotUI(Sprite stampImage)
    {
        if (stampImage != null)
            _stampImage.sprite = stampImage;
    }
}

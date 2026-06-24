using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StampSlotUI : MonoBehaviour
{
    public Image stampImage;
    public StampInstance stampInstance;

    public void SetUpStampSlotUI(Sprite sp)
    {
        if (stampImage != null)
            stampImage.sprite = sp;
    }
}

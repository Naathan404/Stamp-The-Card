using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StampSlotUI : MonoBehaviour
{
    public Image stampImage;
    public Image frameImage;
    public StampInstance stampInstance;

    public void SetUpStampSlotUI(Sprite sp, Sprite frame, StampInstance stampInstance)
    {
        if (stampImage != null)
            stampImage.sprite = sp;
        if (frameImage != null)
            frameImage.sprite = frame;
        this.stampInstance = stampInstance;
    }
}

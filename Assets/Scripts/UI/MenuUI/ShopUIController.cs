using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public void BackToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }
}

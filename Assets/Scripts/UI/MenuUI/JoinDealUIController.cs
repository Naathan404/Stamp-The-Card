using UnityEngine;

public class JoinDealUIController : MonoBehaviour
{
    public void BackToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }
}

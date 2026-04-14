using UnityEngine;

public class SettingUIController : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }
}

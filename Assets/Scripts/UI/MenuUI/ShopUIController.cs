using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }
}

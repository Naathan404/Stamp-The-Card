using UnityEngine;

public class AccountUIController : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }
}

using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneTransitionManager.Instance.LoadSceneAsync(sceneName);
    }

    public void Quit()
    {
       Application.Quit();
    }    
}

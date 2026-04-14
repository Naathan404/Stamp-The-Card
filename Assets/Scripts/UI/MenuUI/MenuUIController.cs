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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync(sceneName);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }    
}

using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    [Header("Menu UI")]
    [SerializeField] private TextMeshProUGUI _soulQuantityTMP;

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

    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.MenuBgMusic);
        Debug.Log($"Phat bg music\n");
        UpdateMenuUI();
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

    private void UpdateMenuUI()
    {
        _soulQuantityTMP.text = LocalPlayerData.Souls.ToString();
    }
}

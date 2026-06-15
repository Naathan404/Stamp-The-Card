using UnityEngine;

public class InventoryUIController : MonoBehaviour 
{
    [SerializeField] private GameObject _stampSlotPrefab;
    [SerializeField] private Transform _stampInventoryPanel;

    private void Start()
    {
        UpdateInventoryUI();
    }

    public void BackToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }

    private void UpdateInventoryUI()
    {

        foreach (var stamp in LocalPlayerData.StampInInventory)
        {
            if (stamp.data == null) continue;

            GameObject currentStamp = Instantiate(_stampSlotPrefab, _stampInventoryPanel);

            StampSlotUI stampSlotInstance = currentStamp.GetComponent<StampSlotUI>();

            if (stampSlotInstance != null)
            {
                stampSlotInstance.SetUpStampSlotUI(stamp.data.stampArt);
            }

        }
    }

    private void OnEnable()
    {
        PlayFabInventoryManager.OnInventoryChanged += UpdateInventoryUI;
    }

    private void OnDisable()
    {
        PlayFabInventoryManager.OnInventoryChanged -= UpdateInventoryUI;
    }
}

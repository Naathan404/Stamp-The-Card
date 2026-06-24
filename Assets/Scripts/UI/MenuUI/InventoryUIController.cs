using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour 
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _stampSlotPrefab;
    [SerializeField] private Transform _stampInventoryPanel;
    [SerializeField] private Transform _stampSelectionPanel;


    [Header("Max slot")]
    [SerializeField] private int _maxInventorySlot = 36;
    [SerializeField] private int _maxSelectionSlot = 9;


    private void Start()
    {
        UpdateInventoryUI();
    }

    public void BackToMenu(string sceneName)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        if (PlayfabManager.Instance != null)
        {
            Debug.Log("Đang đồng bộ Balo lên PlayFab...");

            PlayfabManager.Instance.SaveSelectedStamps(() => 
            {
                SceneTransitionManager.Instance.LoadSceneAsync(sceneName);
            });
        }
        else
        {
            SceneTransitionManager.Instance.LoadSceneAsync(sceneName);
        }
    }

    public void SortInventoryPanelUI()
    {
        SortPanel(_stampInventoryPanel);
        SortPanel(_stampSelectionPanel);

        Canvas.ForceUpdateCanvases();
        
        if (_stampInventoryPanel != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_stampInventoryPanel.GetComponent<RectTransform>());
            
        if (_stampSelectionPanel != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_stampSelectionPanel.GetComponent<RectTransform>());
    }

    private void SortPanel(Transform panel)
    {
        if (panel == null) return;

        StampSlotUI[] currentUIItems = panel.GetComponentsInChildren<StampSlotUI>();

        var sortedUIItems = currentUIItems
            .Where(uiSlot => uiSlot != null && uiSlot.stampInstance != null && uiSlot.stampInstance.data != null)
            .OrderByDescending(uiSlot => uiSlot.stampInstance.data.stampRank)
            .ThenBy(uiSlot => uiSlot.stampInstance.data.stampID)
            .ToList();

        for (int i = 0; i < sortedUIItems.Count; i++)
        {
            sortedUIItems[i].transform.SetSiblingIndex(i);
        }
    }

    // public void UpdateInventoryUI()
    // {
    //     ClearPanel(_stampSelectionPanel);
    //     ClearPanel(_stampInventoryPanel);

    //     foreach (var stamp in LocalPlayerData.StampInInventory)
    //     {
    //         if (stamp.data == null) continue;

    //         bool isSelected = LocalPlayerData.SelectedStamps.Any(s => s.stampInstanceID == stamp.stampInstanceID);  
    //         Transform targetPanel = isSelected ? _stampSelectionPanel : _stampInventoryPanel;
    //         int maxSlot = isSelected ? _maxSelectionSlot : _maxInventorySlot;

    //         if (targetPanel.childCount >= maxSlot)
    //         {
    //             Debug.Log("Panel khong du suc chua!");
    //             continue;
    //         }

    //         GameObject currentStamp = Instantiate(_stampSlotPrefab, targetPanel);
    //         currentStamp.transform.localScale = Vector3.one;
    //         StampSlotUI stampSlotUI = currentStamp.GetComponent<StampSlotUI>();

    //         if (stampSlotUI != null)
    //         {
    //             stampSlotUI.stampInstance = stamp;
    //             stampSlotUI.SetUpStampSlotUI(stamp.data.stampArt, stamp);
    //         }
    //     }

    //     SortInventoryPanelUI();
    // }

    public void UpdateInventoryUI()
    {
        StartCoroutine(InitAndSortRoutine());
    }

    private IEnumerator InitAndSortRoutine()
    {
        ClearPanel(_stampSelectionPanel);
        ClearPanel(_stampInventoryPanel);

        foreach (var stamp in LocalPlayerData.StampInInventory)
        {
            if (stamp.data == null) continue;

            bool isSelected = LocalPlayerData.SelectedStamps.Any(s => s.stampInstanceID == stamp.stampInstanceID);  
            Transform targetPanel = isSelected ? _stampSelectionPanel : _stampInventoryPanel;
            int maxSlot = isSelected ? _maxSelectionSlot : _maxInventorySlot;

            if (targetPanel.childCount >= maxSlot) continue;

            GameObject currentStamp = Instantiate(_stampSlotPrefab, targetPanel);
            
            currentStamp.transform.localScale = Vector3.one;

            StampSlotUI stampSlotUI = currentStamp.GetComponent<StampSlotUI>();
            if (stampSlotUI != null)
            {
                stampSlotUI.stampInstance = stamp;
                stampSlotUI.SetUpStampSlotUI(stamp.data.stampArt, stamp);
            }
        }

        yield return new WaitForEndOfFrame();
        SortInventoryPanelUI();
    }


    private void ClearPanel(Transform panel)
    {
        if (panel == null) return;

        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            GameObject child = panel.GetChild(i).gameObject;

            child.SetActive(false);
            Destroy(child);
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

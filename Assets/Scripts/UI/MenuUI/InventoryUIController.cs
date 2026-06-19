using System.Linq;
using UnityEngine;

public class InventoryUIController : MonoBehaviour 
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _stampSlotPrefab;
    [SerializeField] private Transform _stampInventoryPanel;
    [SerializeField] private Transform _stampSelectionPanel;


    [Header("Max slot")]
    [SerializeField] private int _maxInventorySlot = 36;
    [SerializeField] private int _maxSelectionSlot = 9;


    private SelectedStampZoneController _controller;

    private void Awake()
    {
        if ( _controller == null )
        {
            _controller = FindAnyObjectByType<SelectedStampZoneController>();
        }
    }

    private void Start()
    {
        UpdateInventoryUI();
    }

    public void BackToMenu()
    {
        if (_controller != null)
        {
            _controller.SaveSelectedStampsToLocal();
            Debug.Log("Da luu danh sach selected stamp vao local");
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }

    private void UpdateInventoryUI()
    {
        //Don dep truoc khi update
        ClearPanel(_stampSelectionPanel);
        ClearPanel(_stampInventoryPanel);


        foreach (var stamp in LocalPlayerData.StampInInventory)
        {
            if (stamp.data == null) continue;

            // Xac dinh panel de hien thi
            bool isSelected = LocalPlayerData.SelectedStamps.Any(s => s.stampInstanceID == stamp.stampInstanceID);  //Kiem tra stamp dang duoc chon de battle
            Transform targetPanel = isSelected ? _stampSelectionPanel : _stampInventoryPanel;
            int maxSlot = isSelected ? _maxSelectionSlot : _maxInventorySlot;

            //Tao stamp slot prefab
            if (targetPanel.childCount >= maxSlot)
            {
                Debug.Log("Panel khong du suc chua!");
                continue;
            }

            GameObject currentStamp = Instantiate(_stampSlotPrefab, targetPanel);
            StampSlotUI stampSlotUI = currentStamp.GetComponent<StampSlotUI>();

            if (stampSlotUI != null)
            {
                stampSlotUI.stampInstance = stamp;
                stampSlotUI.SetUpStampSlotUI(stamp.data.stampArt);
            }
        }
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

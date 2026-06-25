using UnityEngine.InputSystem;
using TMPro;
using UnityEngine;


public class ShopUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _soulQuantity;

    [Header("Tooltip Elements")]
    [SerializeField] private GameObject _tooltipPanel;
    [SerializeField] private TextMeshProUGUI _tooltipText;

    private void Start()
    {
        UpdateShopUI();
    }

    private void Update()
    {
        if (_tooltipPanel != null && _tooltipPanel.activeSelf)
        {
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                _tooltipPanel.transform.position = mousePos + new Vector2(50f, -50f);
            }
        }
    }

    public void ShowTooltip(BundleClickable.BundleType type)
    {
        if (_tooltipPanel == null || _tooltipText == null) return;

        _tooltipPanel.SetActive(true);

        switch (type)
        {
            case BundleClickable.BundleType.Normal:
                _tooltipText.text = $"<color={GameConstants.STAMP_RARE_HEX}><b>BASIC CONTRACT</b></color>\n" +
                                    "Common: 70%\nRare: 25%\nEpic: 5%";
                break;
            case BundleClickable.BundleType.Medium:
                _tooltipText.text = $"<color={GameConstants.STAMP_EPIC_HEX}><b>ADVANCED CONTRACT</b></color>\n" +
                                    "Common: 30%\nRare: 50%\nEpic: 20%";
                break;
            case BundleClickable.BundleType.Large:
                _tooltipText.text = $"<color={GameConstants.STAMP_LEGENDARY_HEX}><b>BLOOD CONTRACT</b></color>\n" +
                                    "Rare: 30%\nEpic: 50%\nLegendary: 20%";
                break;
        }
    }

    public void HideTooltip()
    {
        if (_tooltipPanel != null) 
            _tooltipPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }

    public void GoToInventory()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Inventory");
    }

    public void OpenNormalStampPack()
    {
        if (GachaAnimationController.Instance.IsGachaRunning) return;
        int cost = 49; 
        if (LocalPlayerData.Souls < cost)
        {
            GachaAnimationController.Instance.Normal_BundleInsufficientSoulsPlayGachaAnimation();
            return; 
        }
        GachaAnimationController.Instance.IsGachaRunning = true;

        LocalPlayerData.Souls -= cost;
        UpdateShopUI();

        PlayfabManager.Instance.BuyNormalPack(
            newItems =>
            {
                if (newItems == null || newItems.Count == 0)
                {
                    Debug.LogError("Giao dich thanh cong nhung khong nhan duoc vat pham tu server!");
                    LocalPlayerData.Souls += cost;
                    UpdateShopUI();
                    GachaAnimationController.Instance.IsGachaRunning = false;
                    return;
                }

                var purchasedItem = newItems[0];
                BaseStampData newItemData = LocalPlayerData.AllStampsDatabase.Find(stamp => 
                    stamp.stampID.ToString().Trim() == purchasedItem.ItemId.Trim()
                );

                Debug.Log($"[ĐIỀU TRA] PlayFab gửi về ID: '{purchasedItem.ItemId}'");
                foreach(var s in LocalPlayerData.AllStampsDatabase) {
                    Debug.Log($"[ĐIỀU TRA] Database đang có thẻ ID: '{s.stampID.ToString()}'");
                }

                if (newItemData != null)
                {
                    bool isNewStamp = true;
                    if (LocalPlayerData.StampInInventory != null)
                    {
                        foreach (var ownedStamp in LocalPlayerData.StampInInventory)
                        {
                            // Nếu ID thẻ vừa mua giống với ID thẻ nào đó đang có trong túi -> Đã sở hữu
                            if (ownedStamp.data.stampID == newItemData.stampID) 
                            {
                                isNewStamp = false;
                                break;
                            }
                        }
                    }

                    // Đổ dữ liệu ra UI
                    GachaAnimationController.Instance.stampResultSprite.sprite = newItemData.stampArt;
                    GachaAnimationController.Instance.stampFrameSprite.sprite = newItemData.frameArt;
                    GachaAnimationController.Instance.stampResultName.text = newItemData.stampName;
                    GachaAnimationController.Instance.stampResultRarity.text = newItemData.stampRank.ToString();
                    GachaAnimationController.Instance.stampResultEffect.text = newItemData.stampEffect;

                    GachaAnimationController.Instance.Normal_BundlePlayGachaAnimation(isNewStamp);
                }
                else
                {
                    Debug.LogError($"[DEBUG] PlayFab trả về ID: '{purchasedItem.ItemId}'. " + 
                                   $"Trong Database có {LocalPlayerData.AllStampsDatabase.Count} thẻ. " +
                                   $"Thử tìm ID 1: {LocalPlayerData.AllStampsDatabase.Find(s => s.stampID == 1)?.stampName}");
                    GachaAnimationController.Instance.IsGachaRunning = false;
                }
            },
            () => 
            {
                LocalPlayerData.Souls += cost;
                UpdateShopUI();
                GachaAnimationController.Instance.IsGachaRunning = false;
                Debug.LogWarning("Lỗi mạng! Đã hoàn lại tiền ảo trên UI.");
            }
        );
    }

    public void OpenMediumStampPack()
    {
        if (GachaAnimationController.Instance.IsGachaRunning) return;
        int cost = 79; 
        if (LocalPlayerData.Souls < cost)
        {
            GachaAnimationController.Instance.Medium_BundleInsufficientSoulsPlayGachaAnimation();
            return; 
        }
        GachaAnimationController.Instance.IsGachaRunning = true;

        LocalPlayerData.Souls -= cost;
        UpdateShopUI();
        PlayfabManager.Instance.BuyMediumPack(
            newItems =>
            {
                if (newItems == null || newItems.Count == 0)
                {
                    Debug.LogError("Giao dich thanh cong nhung khong nhan duoc vat pham tu server!");
                    LocalPlayerData.Souls += cost;
                    UpdateShopUI();
                    GachaAnimationController.Instance.IsGachaRunning = false;
                    return;
                }

                var purchasedItem = newItems[0];
                BaseStampData newItemData = LocalPlayerData.AllStampsDatabase.Find(stamp => 
                    stamp.stampID.ToString().Trim() == purchasedItem.ItemId.Trim()
                );

                if (newItemData != null)
                {
                    bool isNewStamp = true;
                    if (LocalPlayerData.StampInInventory != null)
                    {
                        foreach (var ownedStamp in LocalPlayerData.StampInInventory)
                        {
                            // Nếu ID thẻ vừa mua giống với ID thẻ nào đó đang có trong túi -> Đã sở hữu
                            if (ownedStamp.data.stampID == newItemData.stampID) 
                            {
                                isNewStamp = false;
                                break;
                            }
                        }
                    }

                    // Đổ dữ liệu ra UI
                    GachaAnimationController.Instance.stampResultSprite.sprite = newItemData.stampArt;
                    GachaAnimationController.Instance.stampFrameSprite.sprite = newItemData.frameArt;
                    GachaAnimationController.Instance.stampResultName.text = newItemData.stampName;
                    GachaAnimationController.Instance.stampResultRarity.text = newItemData.stampRank.ToString();
                    GachaAnimationController.Instance.stampResultEffect.text = newItemData.stampEffect;

                    GachaAnimationController.Instance.Medium_BundlePlayGachaAnimation(isNewStamp);
                }
                else
                {
                    Debug.LogError($"[DEBUG] PlayFab trả về ID: '{purchasedItem.ItemId}'. " + 
                                   $"Trong Database có {LocalPlayerData.AllStampsDatabase.Count} thẻ. " +
                                   $"Thử tìm ID 1: {LocalPlayerData.AllStampsDatabase.Find(s => s.stampID == 1)?.stampName}");
                    GachaAnimationController.Instance.IsGachaRunning = false;
                }
            },
            () => 
            {
                LocalPlayerData.Souls += cost;
                UpdateShopUI();
                GachaAnimationController.Instance.IsGachaRunning = false;
                Debug.LogWarning("Lỗi mạng! Đã hoàn lại tiền ảo trên UI.");
            }
        );
    }

    public void OpenLargeStampPack()
    {
        if (GachaAnimationController.Instance.IsGachaRunning) return;
        int cost = 129; 
        if (LocalPlayerData.Souls < cost)
        {
            GachaAnimationController.Instance.Large_BundleInsufficientSoulsPlayGachaAnimation();
            return; 
        }
        GachaAnimationController.Instance.IsGachaRunning = true;

        LocalPlayerData.Souls -= cost;
        UpdateShopUI();
        PlayfabManager.Instance.BuyLargePack(
            newItems =>
            {
                if (newItems == null || newItems.Count == 0)
                {
                    Debug.LogError("Giao dich thanh cong nhung khong nhan duoc vat pham tu server!");
                    LocalPlayerData.Souls += cost;
                    UpdateShopUI();
                    GachaAnimationController.Instance.IsGachaRunning = false;
                    return;
                }

                var purchasedItem = newItems[0];
                BaseStampData newItemData = LocalPlayerData.AllStampsDatabase.Find(stamp => 
                    stamp.stampID.ToString().Trim() == purchasedItem.ItemId.Trim()
                );

                if (newItemData != null)
                {
                    bool isNewStamp = true;
                    if (LocalPlayerData.StampInInventory != null)
                    {
                        foreach (var ownedStamp in LocalPlayerData.StampInInventory)
                        {
                            // Nếu ID thẻ vừa mua giống với ID thẻ nào đó đang có trong túi -> Đã sở hữu
                            if (ownedStamp.data.stampID == newItemData.stampID) 
                            {
                                isNewStamp = false;
                                break;
                            }
                        }
                    }

                    GachaAnimationController.Instance.stampResultSprite.sprite = newItemData.stampArt;
                    GachaAnimationController.Instance.stampFrameSprite.sprite = newItemData.frameArt;
                    GachaAnimationController.Instance.stampResultName.text = newItemData.stampName;
                    GachaAnimationController.Instance.stampResultRarity.text = newItemData.stampRank.ToString();
                    GachaAnimationController.Instance.stampResultEffect.text = newItemData.stampEffect;

                    GachaAnimationController.Instance.Large_BundlePlayGachaAnimation(isNewStamp);
                }
                else
                {
                    Debug.LogError($"[DEBUG] PlayFab trả về ID: '{purchasedItem.ItemId}'. " + 
                                   $"Trong Database có {LocalPlayerData.AllStampsDatabase.Count} thẻ. " +
                                   $"Thử tìm ID 1: {LocalPlayerData.AllStampsDatabase.Find(s => s.stampID == 1)?.stampName}");
                    GachaAnimationController.Instance.IsGachaRunning = false;
                }
            },
            () => 
            {
                LocalPlayerData.Souls += cost;
                UpdateShopUI();
                GachaAnimationController.Instance.IsGachaRunning = false;
                Debug.LogWarning("Lỗi mạng! Đã hoàn lại tiền ảo trên UI.");
            }
        );
    }

    public void UpdateShopUI()
    {
        _soulQuantity.text = LocalPlayerData.Souls.ToString();
    }
}

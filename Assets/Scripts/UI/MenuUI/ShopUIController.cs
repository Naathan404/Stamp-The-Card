using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;


public class ShopUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _soulQuantity;

    private void Start()
    {
        UpdateShopUI();
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
        PlayfabManager.Instance.BuyNormalPack(
            newItems =>
            {
                if (newItems == null || newItems.Count == 0)
                {
                    Debug.LogError("Giao dich thanh cong nhung khong nhan duoc vat pham tu server!");
                    return;
                }

                BaseStampData newItemData = null;

                foreach (var item in newItems)
                {
                    newItemData = LocalPlayerData.AllStampsDatabase.Find(stamp => stamp.stampID.ToString() == item.ItemId);

                    if (newItemData != null)
                        break;
                }


                if (newItemData != null)
                {
                    GachaAnimationController.Instance.stampResultSprite.sprite = newItemData.stampArt;
                    GachaAnimationController.Instance.stampResultName.text = newItemData.stampName;
                    GachaAnimationController.Instance.stampResultRarity.text = newItemData.stampRank.ToString();
                    GachaAnimationController.Instance.stampResultEffect.text = newItemData.stampEffect;

                    GachaAnimationController.Instance.Normal_BundlePlayGachaAnimation();
                }
                else
                {
                    Debug.LogError("Khong tim thay item tra ve tu server");
                }
            }
        );
    }

    public void OpenMediumStampPack()
    {
        PlayfabManager.Instance.BuyMediumPack(
            newItems =>
            {
                if (newItems == null || newItems.Count == 0)
                {
                    Debug.LogError("Giao dich thanh cong nhung khong nhan duoc vat pham tu server!");
                    return;
                }

                BaseStampData newItemData = null;

                foreach (var item in newItems)
                {
                    newItemData = LocalPlayerData.AllStampsDatabase.Find(stamp => stamp.stampID.ToString() == item.ItemId);

                    if (newItemData != null)
                        break;
                }


                if (newItemData != null)
                {
                    GachaAnimationController.Instance.stampResultSprite.sprite = newItemData.stampArt;
                    GachaAnimationController.Instance.stampResultName.text = newItemData.stampName;
                    GachaAnimationController.Instance.stampResultRarity.text = newItemData.stampRank.ToString();
                    GachaAnimationController.Instance.stampResultEffect.text = newItemData.stampEffect;

                    GachaAnimationController.Instance.Medium_BundlePlayGachaAnimation();
                }
                else
                {
                    Debug.LogError("Khong tim thay item tra ve tu server");
                }
            }
        );
    }

    public void OpenLargeStampPack()
    {
        PlayfabManager.Instance.BuyLargePack(
            newItems =>
            {
                if (newItems == null || newItems.Count == 0)
                {
                    Debug.LogError("Giao dich thanh cong nhung khong nhan duoc vat pham tu server!");
                    return;
                }

                BaseStampData newItemData = null;

                foreach (var item in newItems)
                {
                    newItemData = LocalPlayerData.AllStampsDatabase.Find(stamp => stamp.stampID.ToString() == item.ItemId);

                    if (newItemData != null)
                        break;
                }


                if (newItemData != null)
                {
                    GachaAnimationController.Instance.stampResultSprite.sprite = newItemData.stampArt;
                    GachaAnimationController.Instance.stampResultName.text = newItemData.stampName;
                    GachaAnimationController.Instance.stampResultRarity.text = newItemData.stampRank.ToString();
                    GachaAnimationController.Instance.stampResultEffect.text = newItemData.stampEffect;

                    GachaAnimationController.Instance.Large_BundlePlayGachaAnimation();
                }
                else
                {
                    Debug.LogError("Khong tim thay item tra ve tu server");
                }
            }
        );
    }

    public void UpdateShopUI()
    {
        _soulQuantity.text = LocalPlayerData.Souls.ToString();
    }
}

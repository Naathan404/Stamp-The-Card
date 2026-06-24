using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager Instance;

    public static event Action OnDataChanged;           // Thong bao da thay doi data de cap nhat lai UI

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Hàm phụ trợ: Xác thực xem mật khẩu hiện tại của User có đúng hay không
    public void VerifyCurrentPassword(string currentPassword, Action onSuccess, Action onError)
    {
        // Sử dụng Email hoặc Username để xác thực lại ngầm (ở đây dùng Username làm ví dụ)
        var request = new LoginWithPlayFabRequest
        {
            Username = LocalPlayerData.Username,
            Password = currentPassword
        };

        PlayFabClientAPI.LoginWithPlayFab(request,
            result => {
                Debug.Log("Xác thực mật khẩu hiện tại chính xác!");
                onSuccess?.Invoke();
            },
            error => {
                Debug.LogError("Xác thực thất bại: Mật khẩu hiện tại không đúng.");
                onError?.Invoke();
            }
        );
    }

    // Cập nhật Username (Đã lồng xác thực mật khẩu bên trong để tăng tính bảo mật)
    public void UpdateUsername(string newName, string currentPassword, Action onSuccess, Action onError)
    {
        // Bước 1: Kiểm tra mật khẩu hiện tại trước
        VerifyCurrentPassword(currentPassword,
            () => {
                // Bước 2: Nếu đúng mật khẩu, tiến hành đổi Username
                var request = new AddUsernamePasswordRequest()
                {
                    Username = newName,
                    Password = currentPassword // Đồng thời cập nhật luôn cụm Auth mới
                };

                PlayFabClientAPI.AddUsernamePassword(request,
                    result => {
                        Debug.Log("Thay doi Username thanh cong!");
                        LocalPlayerData.Username = newName;
                        OnDataChanged?.Invoke();
                        onSuccess?.Invoke();
                    },
                    error => {
                        Debug.LogError("Thay doi Username that bai: " + error.GenerateErrorReport());
                        onError?.Invoke();
                    }
                );
            },
            () => {
                // Nếu mật khẩu hiện tại nhập vào bị sai
                onError?.Invoke();
            }
        );
    }

    // Đổi mật khẩu tài khoản (Đã lồng xác thực mật khẩu cũ bên trong)
    public void ChangePassword(string currentPassword, string newPassword, Action onSuccess, Action onError)
    {
        // Bước 1: Kiểm tra mật khẩu cũ trước
        VerifyCurrentPassword(currentPassword,
            () => {
                // Bước 2: Nếu mật khẩu cũ đúng, ghi đè bằng mật khẩu mới
                var request = new AddUsernamePasswordRequest()
                {
                    Username = LocalPlayerData.Username,
                    Password = newPassword
                };

                PlayFabClientAPI.AddUsernamePassword(request,
                    result => {
                        Debug.Log("Doi mat khau thanh cong!");
                        onSuccess?.Invoke();
                    },
                    error => {
                        Debug.LogError("Doi mat khau that bai: " + error.GenerateErrorReport());
                        onError?.Invoke();
                    }
                );
            },
            () => {
                // Nếu mật khẩu cũ nhập vào bị sai
                onError?.Invoke();
            }
        );
    }

    //Cap nhat display name
    public void UpdateDisplayName(string name, Action onSuccess, Action onError)
    {
        var request = new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = name
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => 
            {
                Debug.Log("Cap nhat Display name thanh cong!");
                LocalPlayerData.DisplayName = result.DisplayName;
                OnDataChanged?.Invoke();
                onSuccess?.Invoke();
            },
            error => 
            {
                if (error.Error == PlayFabErrorCode.NameNotAvailable)
                {
                    onError?.Invoke();
                }
                else
                {
                    onError?.Invoke();
                }

                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }
    
    //Cap nhat thong so tran dau
    public void UpdateStatistics(int totalWins, int totalLoses, int rankPoints)
    {
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "TotalWins", Value = totalWins},
                new StatisticUpdate { StatisticName = "TotalLoses", Value = totalLoses},
                new StatisticUpdate { StatisticName = "RankPoints", Value = rankPoints }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result =>
            {
                Debug.Log("Cap nhat statistics thanh cong!");
                OnDataChanged?.Invoke();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    //Cong va tru soul
    public void AddSoul(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "SL",
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request,
            result => {
                LocalPlayerData.Souls = result.Balance;
                Debug.Log("Da cong souls. So du moi: " + result.Balance);
                OnDataChanged?.Invoke();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }
    public void SubtractSoul(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "SL",
            Amount = amount
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result =>
            {
                LocalPlayerData.Souls = result.Balance;
                Debug.Log("Da tru souls. So du moi: " + result.Balance);
                OnDataChanged?.Invoke();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    //Cap phat stamp co ban cho new player
    public void GrantStamp()
    {
        var request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "grantStamps",
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                Debug.Log("Cap phat stamp co ban thanh cong!");
                PlayFabInventoryManager.Instance.GetPlayerInventory();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // Ham goi khi player thuc hien "Giao keo thuong"
    public void BuyNormalPack(Action<List<ItemInstance>> onSuccess)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_normal_pack",
            Price = 49,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            result =>
            {
                Debug.Log("Thuc hien giao keo thanh cong!");
                LocalPlayerData.Souls -= 49;

                PlayFabInventoryManager.Instance.GetPlayerInventory();

                onSuccess?.Invoke(result.Items);
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Normal_BundleInsufficientSoulsPlayGachaAnimation();
                }
                else
                    Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    // Ham goi khi player thuc hien "Giao keo vua"
    public void BuyMediumPack(Action<List<ItemInstance>> onSuccess)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_medium_pack",
            Price = 79,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            result =>
            {
                Debug.Log("Thuc hien giao keo thanh cong!");
                LocalPlayerData.Souls -= 79;

                PlayFabInventoryManager.Instance.GetPlayerInventory();

                onSuccess?.Invoke(result.Items);
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Medium_BundleInsufficientSoulsPlayGachaAnimation();
                }
                else
                    Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    // Ham goi khi player thuc hien "Giao keo to"
    public void BuyLargePack(Action<List<ItemInstance>> onSuccess)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_large_pack",
            Price = 129,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            result =>
            {
                Debug.Log("Thuc hien giao keo thanh cong!");
                LocalPlayerData.Souls -= 129;

                PlayFabInventoryManager.Instance.GetPlayerInventory();

                onSuccess?.Invoke(result.Items);
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Large_BundleInsufficientSoulsPlayGachaAnimation();
                }
                else
                    Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    //Ham luu stamp player da chon de battle
    public void SaveSelectedStamps(Action onComplete = null)
    {
        //Lay ra selected stamp ID de gui len Playfab
        List<string> selectedStampIDs = new List<string>();
        if (LocalPlayerData.SelectedStamps != null)
        {
            foreach (var stamp in LocalPlayerData.SelectedStamps)
            {
                selectedStampIDs.Add(stamp.stampInstanceID);
            }
        }

        //Dong goi du lieu thanh file Json
        string saveSelectedStampsData = PlayFab.Json.PlayFabSimpleJson.SerializeObject(selectedStampIDs);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"SelectedStamps", saveSelectedStampsData}
            },
            Permission = UserDataPermission.Private
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => 
            {
                Debug.Log("Luu selected stamp thanh cong!");
                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                onComplete?.Invoke();
            }
        );
    }

    //Ham load stamp da chon khi dang nhap
    public void LoadSelectedStamps(Action onComplete = null)
    {
        var request = new GetUserDataRequest();

        PlayFabClientAPI.GetUserData(
            request,
            result =>
            {
               LocalPlayerData.SelectedStamps.Clear();

                if (result.Data != null && result.Data.ContainsKey("SelectedStamps"))
                {
                    string jsonString = result.Data["SelectedStamps"].Value;

                    List<string> selectedStampIDs = PlayFab.Json.PlayFabSimpleJson.DeserializeObject<List<string>>(jsonString);
                    foreach (string ID in selectedStampIDs)
                    {
                        foreach (var s in LocalPlayerData.StampInInventory)
                        {
                            string inventoryInstanceID = s.stampInstanceID;

                            if (inventoryInstanceID == ID)
                            {
                                LocalPlayerData.SelectedStamps.Add(s);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Player chua co du lieu selected stamp!");
                }
                onComplete?.Invoke();
            },

            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                onComplete?.Invoke();
            }
        );
    }
}

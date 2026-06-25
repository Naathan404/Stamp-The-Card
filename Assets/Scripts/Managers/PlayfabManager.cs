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
        // Sử dụng Email hoặc Username để xác thực lại ngầm
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
    public void UpdateUsername(string newName, Action onSuccess, Action onError)
    {
        
        var request = new AddUsernamePasswordRequest()
        {
            Username = newName,
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
          
    }

    // Đổi mật khẩu tài khoản (Đã lồng xác thực mật khẩu cũ bên trong)
    // Thay thế hoàn toàn hàm ChangePassword cũ trong PlayfabManager.cs bằng hàm này:
    public void ChangePassword(Action onSuccess, Action onError)
    {
        // Bước 1: Gọi API lấy Email của người chơi hiện tại từ Server về trước
        var accountInfoRequest = new GetAccountInfoRequest();

        PlayFabClientAPI.GetAccountInfo(accountInfoRequest,
            accountResult =>
            {
                if (accountResult.AccountInfo != null &&
                    accountResult.AccountInfo.PrivateInfo != null &&
                    !string.IsNullOrEmpty(accountResult.AccountInfo.PrivateInfo.Email))
                {
                    string userEmail = accountResult.AccountInfo.PrivateInfo.Email;

                    // Bước 2: Dùng API chuẩn của PlayFab để gửi Email khôi phục/đổi mật khẩu
                    var recoveryRequest = new SendAccountRecoveryEmailRequest
                    {
                        Email = userEmail,
                        TitleId = PlayFabSettings.TitleId // Tự động lấy Title ID đang cấu hình trong Unity
                    };

                    PlayFabClientAPI.SendAccountRecoveryEmail(recoveryRequest,
                        result =>
                        {
                            Debug.Log("Đã gửi link đổi mật khẩu vào Email của người chơi thành công!");
                            onSuccess?.Invoke();
                            // Khi thành công, UI bên AccountUIController sẽ nhảy vào khối onSuccess.
                            // Bạn có thể đổi dòng Debug.Log bên UI thành: "Vui lòng kiểm tra Email để đặt lại mật khẩu!"
                        },
                        error =>
                        {
                            Debug.LogError("Gửi email đặt lại mật khẩu thất bại: " + error.GenerateErrorReport());
                            onError?.Invoke();
                        }
                    );
                }
                else
                {
                    Debug.LogError("Tài khoản này chưa được liên kết Email trên PlayFab, không thể gửi yêu cầu!");
                    onError?.Invoke();
                }
            },
            accountError =>
            {
                Debug.LogError("Không thể lấy thông tin tài khoản từ Server: " + accountError.GenerateErrorReport());
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
    public void BuyNormalPack(Action<List<ItemInstance>> onSuccess, Action onError = null)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_normal_pack",
            Price = 49,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            // result =>
            // {
            //     Debug.Log("Thuc hien giao keo thanh cong!");
            //     // LocalPlayerData.Souls -= 49;

            //     PlayFabInventoryManager.Instance.GetPlayerInventory();
            //     onSuccess?.Invoke(result.Items);
            // },
            result =>
            {
                Debug.Log("Mua hộp thành công! Đang tiến hành khui hộp...");

                // Lấy cái ID độc nhất của cái hộp vừa mua
                string containerInstanceId = result.Items[0].ItemInstanceId;

                // BƯỚC 2: GỌI LỆNH KHUI HỘP
                var unlockRequest = new UnlockContainerInstanceRequest()
                {
                    CatalogVersion = "MainCatalog",
                    ContainerItemInstanceId = containerInstanceId
                };

                PlayFabClientAPI.UnlockContainerInstance(unlockRequest,
                    unlockResult => 
                    {
                        Debug.Log("Khui hộp thành công! Nhận được thẻ bài mới.");
                        
                        // Cập nhật lại kho đồ ngầm bên dưới
                        PlayFabInventoryManager.Instance.GetPlayerInventory();

                        // 🌟 TRẢ VỀ LÁ BÀI NẰM BÊN TRONG HỘP (GrantedItems) CHỨ KHÔNG PHẢI CÁI HỘP
                        onSuccess?.Invoke(unlockResult.GrantedItems);
                    },
                    unlockError => 
                    {
                        Debug.LogError("Lỗi khi khui hộp: " + unlockError.GenerateErrorReport());
                        onError?.Invoke();
                    }
                );
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Normal_BundleInsufficientSoulsPlayGachaAnimation();
                    onError?.Invoke(); 
                }
                else
                {
                    Debug.LogError(error.GenerateErrorReport());
                    onError?.Invoke();
                }
            }
        );
    }

    // Ham goi khi player thuc hien "Giao keo vua"
    public void BuyMediumPack(Action<List<ItemInstance>> onSuccess, Action onError = null)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_medium_pack",
            Price = 79,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            // result =>
            // {
            //     Debug.Log("Thuc hien giao keo thanh cong!");
            //     // LocalPlayerData.Souls -= 79;

            //     PlayFabInventoryManager.Instance.GetPlayerInventory();

            //     onSuccess?.Invoke(result.Items);
            // },
            result =>
            {
                Debug.Log("Mua hộp thành công! Đang tiến hành khui hộp...");

                // Lấy cái ID độc nhất của cái hộp vừa mua
                string containerInstanceId = result.Items[0].ItemInstanceId;

                // BƯỚC 2: GỌI LỆNH KHUI HỘP
                var unlockRequest = new UnlockContainerInstanceRequest()
                {
                    CatalogVersion = "MainCatalog",
                    ContainerItemInstanceId = containerInstanceId
                };

                PlayFabClientAPI.UnlockContainerInstance(unlockRequest,
                    unlockResult => 
                    {
                        Debug.Log("Khui hộp thành công! Nhận được thẻ bài mới.");
                        
                        // Cập nhật lại kho đồ ngầm bên dưới
                        PlayFabInventoryManager.Instance.GetPlayerInventory();

                        // 🌟 TRẢ VỀ LÁ BÀI NẰM BÊN TRONG HỘP (GrantedItems) CHỨ KHÔNG PHẢI CÁI HỘP
                        onSuccess?.Invoke(unlockResult.GrantedItems);
                    },
                    unlockError => 
                    {
                        Debug.LogError("Lỗi khi khui hộp: " + unlockError.GenerateErrorReport());
                        onError?.Invoke();
                    }
                );
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Medium_BundleInsufficientSoulsPlayGachaAnimation();
                    onError?.Invoke(); 
                }
                else
                {
                    Debug.LogError(error.GenerateErrorReport());
                    onError?.Invoke();
                }
            }
        );
    }

    // Ham goi khi player thuc hien "Giao keo to"
    public void BuyLargePack(Action<List<ItemInstance>> onSuccess, Action onError = null)
    {
        var request = new PurchaseItemRequest()
        {
            CatalogVersion = "MainCatalog",
            ItemId = "bundle_large_pack",
            Price = 129,
            VirtualCurrency = "SL"
        };

        PlayFabClientAPI.PurchaseItem(request,
            // result =>
            // {
            //     Debug.Log("Thuc hien giao keo thanh cong!");
            //     // LocalPlayerData.Souls -= 129;

            //     PlayFabInventoryManager.Instance.GetPlayerInventory();

            //     onSuccess?.Invoke(result.Items);
            // },
            result =>
            {
                Debug.Log("Mua hộp thành công! Đang tiến hành khui hộp...");

                // Lấy cái ID độc nhất của cái hộp vừa mua
                string containerInstanceId = result.Items[0].ItemInstanceId;

                // BƯỚC 2: GỌI LỆNH KHUI HỘP
                var unlockRequest = new UnlockContainerInstanceRequest()
                {
                    CatalogVersion = "MainCatalog",
                    ContainerItemInstanceId = containerInstanceId
                };

                PlayFabClientAPI.UnlockContainerInstance(unlockRequest,
                    unlockResult => 
                    {
                        Debug.Log("Khui hộp thành công! Nhận được thẻ bài mới.");
                        
                        // Cập nhật lại kho đồ ngầm bên dưới
                        PlayFabInventoryManager.Instance.GetPlayerInventory();

                        // 🌟 TRẢ VỀ LÁ BÀI NẰM BÊN TRONG HỘP (GrantedItems) CHỨ KHÔNG PHẢI CÁI HỘP
                        onSuccess?.Invoke(unlockResult.GrantedItems);
                    },
                    unlockError => 
                    {
                        Debug.LogError("Lỗi khi khui hộp: " + unlockError.GenerateErrorReport());
                        onError?.Invoke();
                    }
                );
            },

            error =>
            {
                if (error.Error == PlayFabErrorCode.InsufficientFunds)
                {
                    Debug.Log("Khong du souls de thuc hien giao keo!");
                    GachaAnimationController.Instance.Large_BundleInsufficientSoulsPlayGachaAnimation();
                    onError?.Invoke(); 
                }
                else
                {
                    Debug.LogError(error.GenerateErrorReport());
                    onError?.Invoke();
                }
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

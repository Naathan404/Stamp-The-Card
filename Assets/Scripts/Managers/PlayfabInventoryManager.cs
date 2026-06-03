using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayFabInventoryManager : MonoBehaviour
{
    public static PlayFabInventoryManager Instance;
    public static event Action OnInventoryChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void GetPlayerInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetInventorySuccess, OnGetInventoryFailed);
    }

    private void OnGetInventorySuccess(GetUserInventoryResult result)
    {
        Debug.Log("Get du lieu inventory thanh cong");

        LocalPlayerData.StampInInventory.Clear();               //reset data

        foreach (ItemInstance item in result.Inventory)
        {
            StampInstance newStamp = new StampInstance();
            newStamp.stampInstanceID = item.ItemInstanceId;
            newStamp.data = LocalPlayerData.AllStampsDatabase.Find(stamp => stamp.stampID.ToString() == item.ItemId);

            LocalPlayerData.StampInInventory.Add(newStamp);

            OnInventoryChanged?.Invoke();
        }
    }

    private void OnGetInventoryFailed(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
}

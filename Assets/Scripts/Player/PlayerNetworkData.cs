using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Fusion;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked] 
    public NetworkString<_32> DisplayName { get; set; }

    [Networked, Capacity(10)]
    public NetworkArray<int> NetSelectedStamps { get; }
    [Networked]
    public int NetSelectedStampsCount { get; set; }

    [Networked] 
    public NetworkBool IsStampSynced { get; set; }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        bool isMe = Object.HasInputAuthority; 

        if (isMe) 
        {
            string savedName = LocalPlayerData.DisplayName; 
            if (HasStateAuthority) DisplayName = savedName; 
            else RPC_SetUsername(savedName); 

            List<string> myStampIDs = new List<string>();
            foreach(var stampInstance in LocalPlayerData.SelectedStamps)
            {
                myStampIDs.Add(stampInstance.data.stampID.ToString());
            }

            string joinedStamps = string.Join(",", myStampIDs);
            if(HasStateAuthority)
            {
                SetStampsOnServer(joinedStamps);
            }
            else
            {
                RPC_SyncStamps(joinedStamps);
            }
        }

        StartCoroutine(WaitAndAssignSeat(isMe));
    }

    private IEnumerator WaitAndAssignSeat(bool isMe)
    {
        UIManager ui = null;

        while (ui == null)
        {
            ui = UIManager.Instance;
            if (ui == null) ui = FindAnyObjectByType<UIManager>();
            if (ui == null) yield return null; 
        }
        
        try 
        {
            UIManager.Instance.SetSeatPosition(this, isMe);

            if (!string.IsNullOrEmpty(DisplayName.ToString()))
            {
                UIManager.Instance.UpdateNameUI(this, isMe);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BÁO ĐỘNG] Crash tại TableManager: {e.Message}\n{e.StackTrace}");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetUsername(string name)
    {
        DisplayName = name; 
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SyncStamps(string joinedStamps)
    {
        SetStampsOnServer(joinedStamps);
    }

    private void SetStampsOnServer(string joinedStamps)
    {
        if (string.IsNullOrEmpty(joinedStamps)) return;
        string[] split = joinedStamps.Split(',');
        NetSelectedStampsCount = split.Length;

        for (int i = 0; i < split.Length; i++)
        {
            if (int.TryParse(split[i], out int stampID))
            {
                NetSelectedStamps.Set(i, stampID);
            }
        }
        IsStampSynced = true;
    }

    public List<int> GetPlayerStampIDs()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < NetSelectedStampsCount; i++)
        {
            list.Add(NetSelectedStamps[i]);
        }
        return list;
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(DisplayName):
                    // Chỉ cập nhật UI nếu bàn đã tồn tại
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateNameUI(this, Object.HasInputAuthority); 
                    }
                    break;
            }
        }
    }
}

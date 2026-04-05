using System.Collections;
using Fusion;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked] 
    public NetworkString<_32> Username { get; set; }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        bool isMe = Object.HasInputAuthority; 

        if (isMe) 
        {
            string savedName = LocalPlayerData.Username; 
            if (HasStateAuthority) Username = savedName; 
            else RPC_SetUsername(savedName); 
        }

        StartCoroutine(WaitAndAssignSeat(isMe));
    }

    private IEnumerator WaitAndAssignSeat(bool isMe)
    {
        TableManager table = null;

        while (table == null)
        {
            table = TableManager.Instance;
            if (table == null) table = FindAnyObjectByType<TableManager>();
            if (table == null) yield return null; 
        }
        
        try 
        {
            table.SetSeatPosition(this, isMe);

            if (!string.IsNullOrEmpty(Username.ToString()))
            {
                table.UpdateNameUI(this, isMe);
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
        Username = name; 
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(Username):
                    // Chỉ cập nhật UI nếu bàn đã tồn tại
                    if (TableManager.Instance != null)
                    {
                        TableManager.Instance.UpdateNameUI(this, Object.HasInputAuthority); 
                    }
                    break;
            }
        }
    }
}

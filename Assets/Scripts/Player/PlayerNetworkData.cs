using System.Collections;
using Fusion;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNetworkData : NetworkBehaviour
{
    [Networked] 
    public NetworkString<_32> DisplayName { get; set; }

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

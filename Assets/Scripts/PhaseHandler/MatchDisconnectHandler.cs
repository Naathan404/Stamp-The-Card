using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using System;

public class MatchDisconnectHandler : NetworkBehaviour, IPlayerLeft, INetworkRunnerCallbacks
{
    private bool _matchEnded = false;

    public void PlayerLeft(PlayerRef player)
    {
        // Kiểm tra đúng là thằng kia thoát, và game chưa kết thúc
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentGameState == GameStateManager.GamePhase.GameOver)
        {
            return; 
        }
        if (player != Runner.LocalPlayer && !_matchEnded)
        {
            Debug.Log("[Mạng] Client đã bỏ trốn! Xử thắng mặc định cho Host.");
            TriggerDefaultWin("COWARD FLED");
        }

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentGameState == GameStateManager.GamePhase.GameOver)
        {
            return; 
        }
        if (!_matchEnded && shutdownReason != ShutdownReason.Ok)
        {
            Debug.Log($"[Match Disconnet] Bàn chơi sụp đổ! Lý do mã lỗi: {shutdownReason}");
            TriggerDefaultWin("COWARD FLED");
        }
    }

    private void TriggerDefaultWin(string reasonMessage)
    {
        _matchEnded = true;

        if (TableVisualManager.Instance != null)
        {
            TableVisualManager.Instance.StopAllCoroutines();
            DG.Tweening.DOTween.KillAll(); 
        }

        if (Runner != null && !Runner.IsShutdown)
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.GameOver);
            }
        }
        else
        {
            Debug.LogWarning("[Mạng] Bỏ qua thao tác đổi Phase vì Server đã sập.");
        }

        int oldRank = LocalPlayerData.RankPoints;
        int oldSouls = LocalPlayerData.Souls;
        int eloChange = 0; 
        int earnedSouls = 0;

        earnedSouls = UnityEngine.Random.Range(15, 20);
        if (oldRank < 500)
            eloChange = UnityEngine.Random.Range(30, 36);
        else if (oldRank < 1000)
            eloChange = UnityEngine.Random.Range(20, 26);
        else if (oldRank < 1500)
            eloChange = UnityEngine.Random.Range(10, 16);

        LocalPlayerData.TotalWins++;
        LocalPlayerData.RankPoints += eloChange;
        LocalPlayerData.Souls += earnedSouls;

        if (PlayfabManager.Instance != null)
        {
            PlayfabManager.Instance.UpdateStatistics(
                LocalPlayerData.TotalWins, 
                LocalPlayerData.TotalLoses, 
                LocalPlayerData.RankPoints
            );
            PlayfabManager.Instance.AddSoul(earnedSouls);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDisconnectWinUI("YOU WON", reasonMessage, oldRank, eloChange, oldSouls, earnedSouls);
        }
        else
        {
            Debug.LogError("Toang rồi! UIManager đã bị null. Cần kiểm tra lại!");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnSceneLoadDone(NetworkRunner runner) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentGameState == GameStateManager.GamePhase.GameOver)
        {
            return; 
        }
        if (player != runner.LocalPlayer && !_matchEnded)
        {
            Debug.Log("[Mạng] Client đã bỏ trốn! Xử thắng mặc định cho Host.");
            TriggerDefaultWin("COWARD FLED");
        }
    }
}
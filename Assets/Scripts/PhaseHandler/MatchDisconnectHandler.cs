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

    // private void TriggerDefaultWin(string reasonMessage)
    // {
    //     _matchEnded = true;

    //     if (GameStateManager.Instance != null)
    //     {
    //         GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.GameOver);
    //     }

    //     if (UIManager.Instance != null)
    //     {
    //         UIManager.Instance.ShowCustomGameOver("YOU WON", reasonMessage);
    //     }
    // }

    private void TriggerDefaultWin(string reasonMessage)
    {
        _matchEnded = true;

        // 1. TẤM KHIÊN BẢO VỆ LỖI TRÀN MẠNG:
        // Chỉ gọi hàm khóa Game Phase nếu Runner VẪN CÒN SỐNG (Trường hợp Client bỏ trốn, Host vẫn giữ phòng).
        // Nếu Host sập mạng (IsShutdown), tuyệt đối KHÔNG đụng vào mấy hàm đồng bộ mạng nữa!

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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowCustomGameOver("You Won", reasonMessage);
        }
        else
        {
            // Nếu lọt vào đây, nghĩa là UIManager của ông đã bị "chết chùm" theo Host.
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
        throw new NotImplementedException();
    }
}
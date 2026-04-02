using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

[RequireComponent(typeof(NetworkRunner))]
public class Launcher : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("NETWORK")]
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacterDic = new Dictionary<PlayerRef, NetworkObject>();

    public async void StartGame(GameMode mode, string roomName)
    {
        _runner = GetComponent<NetworkRunner>();
        // Create the Fusion runner and let it know that we will be providing user input
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs
        {
           GameMode = mode,
           SessionName = roomName,
           PlayerCount = 2,
           SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }


    #region Interface Implementation
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // if (Input.GetKey(KeyCode.W))
        //     data.direction += Vector3.forward;

        // if (Input.GetKey(KeyCode.S))
        //     data.direction += Vector3.back;

        // if (Input.GetKey(KeyCode.A))
        //     data.direction += Vector3.left;

        // if (Input.GetKey(KeyCode.D))
        //     data.direction += Vector3.right;

        input.Set(data);
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // nếu máy đang chạy là server
        if(runner.IsServer)
        {
            // NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, player);
            // _spawnedCharacterDic.Add(player, networkPlayerObject);
            // if(runner.ActivePlayers.Count() == 2)
            // {
            //     runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
            // }
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                // KHÔNG Spawn ở đây. Chỉ đếm xem đủ 2 người chưa để chuyển Scene
                if(runner.ActivePlayers.Count() == 2)
                {
                    Debug.Log("Đã đủ 2 người! Đang tải Scene GamePlay...");
                    runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
                }
            }
            // Nếu người chơi bị rớt mạng và vào lại khi game đang diễn ra (Scene 1)
            else if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, player);
                _spawnedCharacterDic.Add(player, networkPlayerObject);
            }            
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(_spawnedCharacterDic.TryGetValue(player, out NetworkObject networkPlayer))
        {
            runner.Despawn(networkObject: networkPlayer);
            _spawnedCharacterDic.Remove(player);
        }
    }

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.IsServer)
        {
            // Khi Host load xong Scene 1 (GamePlay), bàn chơi đã sẵn sàng
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Debug.Log("Scene GamePlay đã tải xong. Bắt đầu Spawn người chơi...");
                
                // Duyệt qua tất cả những người đang có trong phòng và Spawn nhân vật cho họ
                foreach (var p in runner.ActivePlayers)
                {
                    NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, p);
                    _spawnedCharacterDic.Add(p, networkPlayerObject);
                }
            }
        }        
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    #endregion
}

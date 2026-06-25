using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

//[RequireComponent(typeof(NetworkRunner))]
public class Launcher : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("NETWORK")]
    private NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacterDic = new Dictionary<PlayerRef, NetworkObject>();

    public static Launcher Instance;

    private bool _isSearchingQuickMatch = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        this.transform.SetParent(null); 
        DontDestroyOnLoad(this.gameObject); 
    }

    public void CreateCustomRoom(GameMode mode, string roomName)
    {
        _isSearchingQuickMatch = false;
        Debug.Log($"Đang tạo phòng: {roomName}");
        StartGame(mode, roomName);
    }

    public void FindQuickMatch()
    {
        _isSearchingQuickMatch = true;
        Debug.Log("Đang tìm phòng ngẫu nhiên...");
        // Truyền SessionName = null để Fusion tự tìm phòng trống
        StartGame(GameMode.AutoHostOrClient, null); 
    }

    public async void StartGame(GameMode mode, string roomName)
    {
        _spawnedCharacterDic.Clear();

        GameObject runnerObj = new GameObject("MySessionRunner");
        DontDestroyOnLoad(runnerObj);

        // Create the Fusion runner and let it know that we will be providing user input
        _runner = runnerObj.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();
        _runner.AddCallbacks(this);

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs
        {
           GameMode = mode,
           SessionName = roomName,
           PlayerCount = 2,
           SceneManager = /*gameObject.AddComponent<NetworkSceneManagerDefault>()*/ sceneManager
        });
    }

    public void CancelMatchmaking()
    {
        if (_runner != null && !_runner.IsShutdown)
        {
            Debug.Log("[Laucher] Hủy tìm trận");
            _runner.Shutdown();
        }
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
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                if(runner.ActivePlayers.Count() == 2)
                {
                    Debug.Log("Đã đủ 2 người! Đang tải Scene GamePlay...");
                    runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
                }
            }
            else if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                // NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, player);
                // _spawnedCharacterDic.Add(player, networkPlayerObject);
                if (!_spawnedCharacterDic.ContainsKey(player))
                {
                    NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, player);
                    _spawnedCharacterDic.Add(player, networkPlayerObject);
                }
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
                    // NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, p);
                    // _spawnedCharacterDic.Add(p, networkPlayerObject);
                    if (!_spawnedCharacterDic.ContainsKey(p))
                    {
                        NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, Vector2.zero, Quaternion.identity, p);
                        _spawnedCharacterDic.Add(p, networkPlayerObject);
                    }
                }
            }
        }        
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (_isSearchingQuickMatch)
        {
            foreach (var session in sessionList)
            {
                if (session.PlayerCount < 2)
                {
                    _isSearchingQuickMatch = false; // Tìm thấy rồi, tắt cờ
                    Debug.Log($"Ghép trận thành công vào: {session.Name}");
                    StartGame(GameMode.Client, session.Name);
                    return;
                }
            }
        }
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        _spawnedCharacterDic.Clear();
        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }
    }

    public void LeaveMatch()
    {
        if (_runner != null && !_runner.IsShutdown)
        {
            Debug.Log("Đang chủ động rời trận. Hàm OnShutdown sẽ tự động đưa bạn về Lobby...");
            _runner.Shutdown(); 
        }
        else 
        {
            
        }
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    #endregion
}

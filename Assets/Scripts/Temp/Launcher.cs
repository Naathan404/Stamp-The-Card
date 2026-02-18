using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        // Kết nối với cấu hình AppID bạn đã dán trong Unity
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("<color=blue>Đang kết nối đến Server...</color>");
    }
    public override void OnConnectedToMaster()
    {
        // base.OnConnectedToMaster();
        Debug.Log("<color=green>Đã kết nối đến Server thành công!</color>");
        PhotonNetwork.JoinLobby();  // tham gia sảnh chờ chung
    }

    public override void OnJoinedLobby()
    {
        // base.OnJoinedLobby();
        // tìm phòng bất kỳ, nếu không có thì tự tạo phòng mới tối đa 2 người
        Debug.Log("<color=yellow>Đã vào Lobby, đang tìm phòng</color>");
        PhotonNetwork.JoinOrCreateRoom("BanChoi1", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        // base.OnJoinedRoom();
        Debug.Log("<color=green>Đã vào phòng thành công!</color> Số người hiện tại: " + PhotonNetwork.CurrentRoom.PlayerCount);
        // có thể khởi tạo trò chơi hoặc tải scene mới ở đây nha, nhắc để nữa quên =)))))
    }

    public override void OnConnected()
    {
        base.OnConnected();
    }
}

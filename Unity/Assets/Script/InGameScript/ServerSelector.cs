//using UnityEngine;
//using Photon.Pun;
//using Photon.Realtime;

//public class ServerSelector : MonoBehaviourPunCallbacks
//{
//    public GameObject chatPanel;
//    public void JoinServer1() => JoinServer("Server1");
//    public void JoinServer2() => JoinServer("Server2");
//    public void JoinServer3() => JoinServer("Server3");
//    public GameObject roomCam;
//    private void JoinServer(string serverName)
//    {
//        if (string.IsNullOrEmpty(serverName))
//        {
//            Debug.LogError("Tên server không được để trống!");
//            return;
//        }

//        RoomOptions options = new RoomOptions { MaxPlayers = 16 };
//        PhotonNetwork.JoinOrCreateRoom(serverName, options, TypedLobby.Default);
//    }

//    public override void OnJoinedRoom()
//    {
//        chatPanel.SetActive(true);
//        base.OnJoinedRoom();
//        roomCam.SetActive(false);
//        Debug.Log("Đã vào phòng: " + PhotonNetwork.CurrentRoom.Name);
//        RoomManager.instance.SpawnPlayer();
//        if (PhotonNetwork.IsMasterClient)
//        {
//            RoomManager.instance.SpawnBox1();
//            RoomManager.instance.StartSpawnBoxLoop(10f);
//        }
//    }
//    private void SpawnBoxRepeat()
//    {
//        RoomManager.instance.SpawnBox1();
//    }
//}

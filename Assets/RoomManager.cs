using UnityEngine;
using Photon.Pun;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;
    
    [Header("Player")]
    public GameObject player;
    public Transform spawnPoint;

    [Header("UI")]
    public GameObject roomCam;
    public GameObject nameUI;
    public GameObject connectingUI;
    public GameObject selectUI; // panel chọn server
    
    private string nickname = "unnamed";
    [HideInInspector]
    public int kills = 0;
    [HideInInspector]
    public int deaths = 0;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeNickname(string name)
    {
        nickname = name;
    }

    public void JoinRoomButtonPressed()
    {
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.NickName = nickname;
        PhotonNetwork.ConnectUsingSettings();

        nameUI.SetActive(false);
        connectingUI.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby. Waiting for server selection...");
        connectingUI.SetActive(false);
        selectUI.SetActive(true);
    }

    // Hàm này nên được gọi trong scene chơi (sau khi JoinOrCreateRoom và LoadLevel xong)
    public void SpawnPlayer()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1f); // delay ngắn cho an toàn

        if (PhotonNetwork.LocalPlayer != null)
        {
            GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
            _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, nickname);
            PhotonNetwork.LocalPlayer.NickName = nickname;
        }
    }

    public void SetHashes()
    {
        try
        {
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
            hash["kills"] = kills;
            hash["deaths"] = deaths;

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        catch
        {
        }
    }
    

}

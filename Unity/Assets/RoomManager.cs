using UnityEngine;
using Photon.Pun;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;
    public GameObject chatPanel;
    [Header("Player")]
    public GameObject player;
    public Transform[] spawnPoints;

    [Header("UI")]
    public GameObject roomCam;
    public GameObject nameUI;
    public GameObject connectingUI;
    public GameObject selectUI;
    [Header("Pickup Prefabs")]
    public GameObject box1Prefab;
    [Header("Box Spawn Points")]
    public Transform[] boxSpawnPoints;
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

    public void SpawnPlayer()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1f); // delay ngắn cho an toàn

        if (PhotonNetwork.LocalPlayer != null)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];
            GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
            _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, nickname);
            PhotonNetwork.LocalPlayer.NickName = nickname;
            _player.GetComponent<Health>().SetInvincible(3f);
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
    public void SpawnBox1()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (boxSpawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, boxSpawnPoints.Length);
        Vector3 spawnPos = boxSpawnPoints[randomIndex].position;

        PhotonNetwork.Instantiate(box1Prefab.name, spawnPos, Quaternion.identity);
    }
    public void StartSpawnBoxLoop(float interval)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating(nameof(SpawnBox1), interval, interval);
        }
    }
}

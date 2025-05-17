using UnityEngine;
using Mirror;
using System.Collections;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager instance;

    [Header("Player")]
    public GameObject playerPrefab;
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
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // --- NICKNAME ---
    public void ChangeNickname(string name)
    {
        nickname = name;
    }

    // --- JOIN ROOM ---
    // Mirror không tự connect, cần tự quản lý kết nối
    public void JoinRoomButtonPressed()
    {
        Debug.Log("Connecting to Mirror Server...");

        // Hiện UI đang kết nối
        nameUI.SetActive(false);
        connectingUI.SetActive(true);

        if (!NetworkManager.singleton.isNetworkActive)
        {
            NetworkManager.singleton.StartClient();
        }
    }

    // Mirror dùng callback trên NetworkManager để biết kết nối thành công
    public void OnConnectedToServer()
    {
        Debug.Log("Connected to Mirror Server!");
        connectingUI.SetActive(false);
        selectUI.SetActive(true);
    }

    // --- SPAWN PLAYER ---
    public void SpawnPlayer()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1f);

        if (!NetworkClient.isConnected || playerPrefab == null) yield break;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // Yêu cầu server spawn player cho client này
        CmdSpawnPlayer(spawnPoint.position, spawnPoint.rotation, nickname);
    }

    [Command]
    private void CmdSpawnPlayer(Vector3 pos, Quaternion rot, string playerName, NetworkConnectionToClient sender = null)
    {
        GameObject _player = Instantiate(playerPrefab, pos, rot);
        NetworkServer.Spawn(_player, sender);

        // Gán nickname (giả sử Player script có phương thức SetNickname)
        Player playerScript = _player.GetComponent<Player>();
        if (playerScript != null)
        {
            //playerScript.SetNickname(playerName);
        }
    }

    // --- UPDATE KILLS & DEATHS ---
    public void SetHashes(int newKills, int newDeaths)
    {
        kills = newKills;
        deaths = newDeaths;

        // Nếu cần gửi lên server hoặc sync thì nên dùng SyncVar hoặc Command trong Player script
    }

    // --- SPAWN BOX ---
    public void SpawnBox1()
    {
        if (!NetworkServer.active) return; // Chỉ server spawn

        if (boxSpawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, boxSpawnPoints.Length);
        Vector3 spawnPos = boxSpawnPoints[randomIndex].position;

        GameObject box = Instantiate(box1Prefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(box);
    }

    public void StartSpawnBoxLoop(float interval)
    {
        if (!NetworkServer.active) return;

        InvokeRepeating(nameof(SpawnBox1), interval, interval);
    }
}

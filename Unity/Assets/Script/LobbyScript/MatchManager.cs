using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Mirror;
using TMPro;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;
    public TMP_Text statusText;
    public static string CurrentNickname;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    [System.Serializable]
    public class CreateMatchResponse
    {
        public string room_code;
        public string status;
    }

    [System.Serializable]
    public class JoinMatchResponse
    {
        public string room_code;
        public string status;
    }

    public void CreateRoom()
    {
        StartCoroutine(CreateRoomCoroutine());
    }

    IEnumerator CreateRoomCoroutine()
    {
        string url = "http://127.0.0.1:8000/matches/create/";
        UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "");
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            MatchManager.CurrentNickname = PlayerPrefs.GetString("nickname", "Unknown");
            CreateMatchResponse response = JsonUtility.FromJson<CreateMatchResponse>(www.downloadHandler.text);
            Debug.Log("Tạo phòng thành công, room_code = " + response.room_code);
            PlayerPrefs.SetString("room_code", response.room_code);
            StartMirrorHost(response.room_code);
        }
        else
        {
            Debug.LogError("Lỗi tạo phòng: " + www.downloadHandler.text);
        }
    }

    void StartMirrorHost(string roomCode)
    {
        NetworkRoomManager roomManager = FindObjectOfType<NetworkRoomManager>();
        //roomManager.roomName = roomCode;

        UnityEngine.SceneManagement.SceneManager.LoadScene("RoomScene");
        roomManager.StartHost();
    }

    public void JoinRoom(string roomCode)
    {
        StartCoroutine(JoinRoomCoroutine(roomCode));
    }

    IEnumerator JoinRoomCoroutine(string roomCode)
    {
        string url = "http://127.0.0.1:8000/matches/join/";
        WWWForm form = new WWWForm();
        form.AddField("room_code", roomCode);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            MatchManager.CurrentNickname = PlayerPrefs.GetString("nickname", "Unknown");
            Debug.Log("Join phòng thành công, room_code = " + roomCode);
            UnityEngine.SceneManagement.SceneManager.LoadScene("RoomScene");
            StartMirrorClient(roomCode);
        }
        else
        {
            Debug.LogError("Lỗi join phòng: " + www.downloadHandler.text);
            statusText.text = "Mã phòng không tồn tại hoặc đã đầy!";
        }
    }

    void StartMirrorClient(string roomCode)
    {
        NetworkRoomManager roomManager = FindObjectOfType<NetworkRoomManager>();
        //roomManager.roomName = roomCode;
        roomManager.networkAddress = "localhost"; // hoặc IP server
        roomManager.StartClient();
    }
}

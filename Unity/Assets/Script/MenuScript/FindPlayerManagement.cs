using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class FindPlayerManager : MonoBehaviour
{
    public static FindPlayerManager instance; // Singleton

    [Header("UI")]
    public TMP_InputField searchInput;
    public TMP_Text countRequest;
    public GameObject[] playerEntries;
    public GameObject playerRequest;
    public GameObject playerAdd;
    public GameObject friendForm;

    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public string nickname;
    }

    [System.Serializable]
    public class PlayerList
    {
        public List<PlayerData> players;
    }

    [System.Serializable]
    public class RequestCountResponse
    {
        public int request_count;
    }

    private void Awake()
    {
        // Setup Singleton
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(GetFriendRequestCount());
    }

    public void OnOpenAddFriend()
    {
        playerAdd.SetActive(true);
    }

    public void OnCloseAddFriend()
    {
        playerAdd.SetActive(false);
    }
    public void OnCloseFriendForm()
    {
        friendForm.SetActive(false);
        Debug.Log("Friend form closed.");
    }
    public void OnOpenFriendRequest()
    {
        playerRequest.SetActive(true);

        // Khi mở UI Request => cập nhật luôn số lượng
        StartCoroutine(GetFriendRequestCount());
    }

    public void OnFindButton()
    {
        string query = searchInput.text;
        StartCoroutine(SearchPlayers(query));
    }

    IEnumerator SearchPlayers(string query)
    {
        string url = "http://127.0.0.1:8000/search-friends/?q=" + UnityWebRequest.EscapeURL(query);
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"players\":" + www.downloadHandler.text + "}";
            PlayerList result = JsonUtility.FromJson<PlayerList>(json);

            for (int i = 0; i < playerEntries.Length; i++)
            {
                if (i < result.players.Count)
                {
                    var player = result.players[i];
                    GameObject entry = playerEntries[i];
                    entry.SetActive(true);

                    TMP_Text text = entry.GetComponentInChildren<TMP_Text>();
                    Button button = entry.GetComponentInChildren<Button>();

                    text.text = player.nickname;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => StartCoroutine(SendFriendRequest(player.id)));
                }
                else
                {
                    playerEntries[i].SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogError("Lỗi tìm người chơi: " + www.downloadHandler.text);
        }
    }

    IEnumerator SendFriendRequest(int playerId)
    {
        string url = "http://127.0.0.1:8000/add-friend/";
        WWWForm form = new WWWForm();
        form.AddField("player_id", playerId);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Kết bạn thành công với ID: " + playerId);

            // Sau khi gửi lời mời => cập nhật số lượng lời mời kết bạn
            StartCoroutine(GetFriendRequestCount());
        }
        else
        {
            Debug.LogError("Không thể kết bạn: " + www.downloadHandler.text);
        }
    }

    public IEnumerator GetFriendRequestCount()
    {
        string url = "http://127.0.0.1:8000/get-friend-request-count/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        string token = PlayerPrefs.GetString("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            www.SetRequestHeader("Authorization", "Bearer " + token);
        }
        else
        {
            Debug.LogError("TOKEN RỖNG!! Không thể gửi request lấy số lượng lời mời.");
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = www.downloadHandler.text;
            RequestCountResponse response = JsonUtility.FromJson<RequestCountResponse>(jsonResponse);
            countRequest.text = response.request_count.ToString();
        }
        else
        {
            Debug.LogError("Lỗi khi lấy số lượng lời mời: " + www.downloadHandler.text);
        }
    }
}

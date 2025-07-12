using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

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
    public GameObject playerChatFriend;
    public Transform listFriendContainer; // Là ListFriend (Panel)
    public GameObject friendItemPrefab;   // Prefab chứa Text để hiện tên bạn
    public string apiUrl = "http://localhost:8000/friends/"; // Sửa nếu cần
    public string jwtToken; // Token JWT lấy từ khi login
    private bool hasLoadedFriends = false;
    private List<GameObject> friendItems = new List<GameObject>();
    private List<int> sentRequestIds = new List<int>();

    IEnumerator GetFriendList()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            List<PlayerData> friends = JsonConvert.DeserializeObject<List<PlayerData>>(json);

            foreach (var friend in friends)
            {
                GameObject item = Instantiate(friendItemPrefab, listFriendContainer);
                item.GetComponent<FriendList>().Setup(friend);
                friendItems.Add(item); // Lưu vào danh sách
            }

        }
        else
        {
            Debug.LogError("Lỗi lấy danh sách bạn bè: " + request.error);
        }
    }
    public void ShowFriendListFromCache()
    {
        foreach (var item in friendItems)
        {
            if (item != null)
                item.SetActive(true);
        }
    }

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
        jwtToken = PlayerPrefs.GetString("access_token");
        LoadSentRequestIds();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            if (!hasLoadedFriends)
            {
                StartCoroutine(LoadFriendsOnce());
            }

            StartCoroutine(GetFriendRequestCount());
        }
        else
        {
            Debug.LogError("JWT token rỗng. Không thể lấy danh sách bạn bè.");
        }
    }
    IEnumerator LoadFriendsOnce()
    {
        yield return StartCoroutine(GetFriendList());
        hasLoadedFriends = true;
    }

    public void AddFriendToList(string friendName)
    {
        GameObject newFriend = Instantiate(friendItemPrefab, listFriendContainer);
        TMP_Text nameText = newFriend.GetComponentInChildren<TMP_Text>();
        if (nameText != null)
        {
            nameText.text = friendName;
        }
    }
    public void LoadFriendsWhenOpen()
    {
        if (string.IsNullOrEmpty(jwtToken))
            jwtToken = PlayerPrefs.GetString("access_token");

        StartCoroutine(GetFriendList());
    }

    public void OnOpenChatFriend()
    {
        playerChatFriend.SetActive(true);
    }
    public void OnCloseChatFriend()
    {
        playerChatFriend.SetActive(false);
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

                    // 🔧 RESET TRẠNG THÁI BUTTON
                    // Kiểm tra xem đã gửi lời mời chưa
                    if (sentRequestIds.Contains(player.id))
                    {
                        // Đã gửi lời mời rồi
                        button.interactable = false;
                        button.GetComponentInChildren<TMP_Text>().text = "Sent";
                    }
                    else
                    {
                        // Chưa gửi lời mời
                        button.interactable = true;
                        button.GetComponentInChildren<TMP_Text>().text = "Add";
                    }

                    button.onClick.RemoveAllListeners();

                    // Chỉ add listener nếu chưa gửi lời mời
                    if (!sentRequestIds.Contains(player.id))
                    {
                        int capturedId = player.id;
                        Button capturedButton = button;

                        capturedButton.onClick.AddListener(() =>
                        {
                            capturedButton.interactable = false;
                            capturedButton.GetComponentInChildren<TMP_Text>().text = "Sent";

                            // Lưu ID đã gửi lời mời
                            sentRequestIds.Add(capturedId);
                            SaveSentRequestIds();

                            StartCoroutine(SendFriendRequest(capturedId));
                        });
                    }
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

    public IEnumerator DeleteFriend(int friendId, GameObject friendItem)
    {
        Debug.Log($"Attempting to delete friend ID: {friendId}");

        // Kiểm tra cơ bản
        if (friendId <= 0 || friendItem == null)
        {
            Debug.LogError("Invalid parameters");
            yield break;
        }

        if (string.IsNullOrEmpty(jwtToken))
            jwtToken = PlayerPrefs.GetString("access_token");

        // Thử exactly như code gốc nhưng với POST
        string url = "http://127.0.0.1:8000/remove-friend/";

        WWWForm form = new WWWForm();
        form.AddField("player_id", friendId);  // Đổi từ friend_id thành player_id

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.SetRequestHeader("Authorization", "Bearer " + jwtToken);

        yield return www.SendWebRequest();

        Debug.Log($"Response Code: {www.responseCode}");
        Debug.Log($"Response: {www.downloadHandler.text}");

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Server responded SUCCESS");

            // Kiểm tra response content để đảm bảo thực sự thành công
            string responseText = www.downloadHandler.text;
            if (responseText.Contains("error") || responseText.Contains("required"))
            {
                Debug.LogError($"❌ Server returned error: {responseText}");
                www.Dispose();
                yield break;
            }

            // Xóa khỏi UI
            if (friendItems != null)
                friendItems.Remove(friendItem);
            Destroy(friendItem);

            Debug.Log("✅ Removed from UI");

            // Reload lại danh sách để đảm bảo sync với server
            Debug.Log("🔄 Reloading friend list...");
            yield return StartCoroutine(GetFriendList());
        }
        else
        {
            Debug.LogError($"❌ Request failed: {www.result}");
            Debug.LogError($"❌ Error: {www.error}");
            Debug.LogError($"❌ Response: {www.downloadHandler?.text}");
        }

        www.Dispose();
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
    // Lưu danh sách ID đã gửi lời mời
    private void SaveSentRequestIds()
    {
        string json = JsonConvert.SerializeObject(sentRequestIds);
        PlayerPrefs.SetString("sent_request_ids", json);
        PlayerPrefs.Save();
    }

    // Load danh sách ID đã gửi lời mời
    private void LoadSentRequestIds()
    {
        string json = PlayerPrefs.GetString("sent_request_ids", "[]");
        sentRequestIds = JsonConvert.DeserializeObject<List<int>>(json);
    }
}
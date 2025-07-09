using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class FriendRequestManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject[] playerEntries; // Các entry UI
    public GameObject friendRequestPanel;

    private const string BASE_URL = "http://127.0.0.1:8000";
    private const string TOKEN_KEY = "access_token";

    void Start()
    {
        LoadFriendRequests();
    }

    public void CloseFriendRequestPanel()
    {
        friendRequestPanel.SetActive(false);
    }

    public void LoadFriendRequests()
    {
        StartCoroutine(GetFriendRequests());
    }

    IEnumerator GetFriendRequests()
    {
        string url = $"{BASE_URL}/get-friend-requests";
        UnityWebRequest www = UnityWebRequest.Get(url);

        AddAuthHeader(www);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = "{\"requests\":" + www.downloadHandler.text + "}";
            Debug.Log("Received Friend Requests: " + jsonResponse);

            FriendRequestList response = JsonUtility.FromJson<FriendRequestList>(jsonResponse);
            UpdateFriendRequestUI(response.requests);
        }
        else
        {
            Debug.LogError("Failed to get friend requests: " + www.error);
        }
    }

    void UpdateFriendRequestUI(FriendRequest[] requests)
    {
        for (int i = 0; i < playerEntries.Length; i++)
        {
            if (i < requests.Length)
            {
                SetupFriendRequestEntry(playerEntries[i], requests[i]);
            }
            else
            {
                playerEntries[i].SetActive(false);
            }
        }
    }

    void SetupFriendRequestEntry(GameObject entry, FriendRequest request)
    {
        entry.SetActive(true);

        TMP_Text nameText = entry.transform.Find("PlayerName")?.GetComponent<TMP_Text>();
        Button acceptBtn = entry.transform.Find("AcceptButton")?.GetComponent<Button>();
        Button rejectBtn = entry.transform.Find("RejectButton")?.GetComponent<Button>();

        if (nameText != null)
            nameText.text = request.fromPlayerName ?? $"Player {request.from_player}";
        else
            Debug.LogWarning("Missing PlayerName in entry UI");

        if (acceptBtn != null)
        {
            acceptBtn.onClick.RemoveAllListeners();
            acceptBtn.onClick.AddListener(() => StartCoroutine(RespondToRequest(request.id, "accept", entry)));
        }

        if (rejectBtn != null)
        {
            rejectBtn.onClick.RemoveAllListeners();
            rejectBtn.onClick.AddListener(() => StartCoroutine(RespondToRequest(request.id, "reject", entry)));
        }
    }

    IEnumerator RespondToRequest(int requestId, string action, GameObject entry)
    {
        string url = $"{BASE_URL}/respond-friend-request/";
        UnityWebRequest www = new UnityWebRequest(url, "POST");

        string jsonData = JsonUtility.ToJson(new RequestData { request_id = requestId, action = action });
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");
        AddAuthHeader(www);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Successfully {action}ed request {requestId}");
            if (entry != null)
                entry.SetActive(false);
        }
        else
        {
            Debug.LogError($"Failed to {action} friend request: {www.error}");
        }
    }

    void AddAuthHeader(UnityWebRequest www)
    {
        string token = PlayerPrefs.GetString(TOKEN_KEY);
        if (!string.IsNullOrEmpty(token))
            www.SetRequestHeader("Authorization", $"Bearer {token}");
    }

    // Data structures
    [System.Serializable]
    public class FriendRequest
    {
        public int id;
        public string fromPlayerName;
        public int from_player;
    }

    [System.Serializable]
    public class FriendRequestList
    {
        public FriendRequest[] requests;
    }

    [System.Serializable]
    public class RequestData
    {
        public int request_id;
        public string action;
    }
}

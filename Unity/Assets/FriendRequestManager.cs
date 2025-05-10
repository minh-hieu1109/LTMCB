using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class FriendRequestManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject[] playerEntries; // Những gameObject chứa tên và button đồng ý, từ chối
    private string apiUrl = "http://127.0.0.1:8000/respond-friend-request/"; // URL của backend
    public GameObject GameObject;

    void Start()
    {
        LoadFriendRequests();
    }

    public void CloseOpenFriendRequest()
    {
        GameObject.SetActive(false);
    }

    // Load danh sách yêu cầu kết bạn từ backend
    public void LoadFriendRequests()
    {
        StartCoroutine(GetFriendRequests());
    }

    // Gửi yêu cầu GET để lấy danh sách yêu cầu kết bạn
    IEnumerator GetFriendRequests()
    {
        string url = "http://127.0.0.1:8000/get-friend-requests"; // URL của API backend
        UnityWebRequest www = UnityWebRequest.Get(url);

        // Thêm Authorization header với token JWT
        string token = PlayerPrefs.GetString("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            www.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"requests\":" + www.downloadHandler.text + "}";

            Debug.Log("Received JSON: " + json); // Thêm log này để kiểm tra dữ liệu nhận được
            FriendRequestList result = JsonUtility.FromJson<FriendRequestList>(json);

            // Duyệt qua các yêu cầu kết bạn và hiển thị chúng trên UI
            for (int i = 0; i < playerEntries.Length; i++)
            {
                if (i < result.requests.Length)
                {
                    var friendRequest = result.requests[i];
                    GameObject entry = playerEntries[i];
                    entry.SetActive(true);

                    // Kiểm tra và gán các thành phần cần thiết trong playerEntries
                    TMP_Text text = entry.transform.Find("PlayerName")?.GetComponent<TMP_Text>();
                    Button acceptButton = entry.transform.Find("AcceptButton")?.GetComponent<Button>();
                    Button rejectButton = entry.transform.Find("RejectButton")?.GetComponent<Button>();

                    if (text != null)
                    {
                        // Lấy tên người gửi từ ID (hoặc từ API nếu bạn có tên người gửi)
                        text.text = "Player " + friendRequest.from_player; // Tạm thay thế bằng ID người gửi nếu chưa có tên
                    }
                    else
                    {
                        Debug.LogError("PlayerName component is missing in player entry " + i);
                    }

                    if (acceptButton != null)
                    {
                        acceptButton.onClick.RemoveAllListeners();
                        acceptButton.onClick.AddListener(() => StartCoroutine(RespondToRequest(friendRequest.id, "accept", entry)));
                    }
                    else
                    {
                        Debug.LogError("AcceptButton is missing in player entry " + i);
                    }

                    if (rejectButton != null)
                    {
                        rejectButton.onClick.RemoveAllListeners();
                        rejectButton.onClick.AddListener(() => StartCoroutine(RespondToRequest(friendRequest.id, "reject", entry)));
                    }
                    else
                    {
                        Debug.LogError("RejectButton is missing in player entry " + i);
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
            Debug.LogError("Error getting friend requests: " + www.downloadHandler.text);
        }
    }


    IEnumerator RespondToRequest(int requestId, string action, GameObject entry)
    {
        string url = apiUrl; // URL API backend
        UnityWebRequest www = new UnityWebRequest(url, "POST");

        // Tạo dữ liệu JSON với RequestData
        string jsonData = JsonUtility.ToJson(new RequestData { request_id = requestId, action = action });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Gửi JSON body
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        // Đặt header Content-Type là application/json
        www.SetRequestHeader("Content-Type", "application/json");

        // Thêm header Authorization nếu có token
        string token = PlayerPrefs.GetString("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            www.SetRequestHeader("Authorization", "Bearer " + token);
        }

        // Gửi request
        yield return www.SendWebRequest();

        // Kiểm tra kết quả
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + www.downloadHandler.text);

            // Ngay lập tức ẩn entry đã được xử lý (chấp nhận hoặc từ chối)
            if (entry != null)
            {
                entry.SetActive(false);  // Hoặc sử dụng Destroy(entry) để xóa hoàn toàn
            }

            // Bạn có thể gọi lại GetFriendRequests() nếu muốn tải lại dữ liệu mới từ server
            // GetFriendRequests();
        }
        else
        {
            Debug.LogError("Error responding to friend request: " + www.downloadHandler.text);
        }
    }


    // Cấu trúc dữ liệu FriendRequest
    [System.Serializable]
    public class FriendRequest
    {
        public int id;
        public string fromPlayerName; // Chắc chắn có tên người gửi
        public int from_player; // ID người gửi
    }

    // Cấu trúc dữ liệu FriendRequestList
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

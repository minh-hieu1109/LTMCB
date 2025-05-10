using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class FindPlayerManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField searchInput;
    public TMP_Text countRequest;
    public GameObject[] playerEntries; // G�n FindPlayer, FindPlayer(1), ...
    public GameObject playerRequest;
    public GameObject playerAdd;
    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public string nickname;
    }

    public class PlayerList
    {
        public List<PlayerData> players;
    }
    public void OnOpenAddFriend()
    {
        playerAdd.SetActive(true);
    }
    public void OnClosrAddFriend()
    {
        playerAdd.SetActive(false);
    }

    public void OnOpenFriendRequest()
    {
        playerRequest.SetActive(true);
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
            Debug.LogError("Loi tim nguoi choii: " + www.downloadHandler.text);
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
        }
        else
        {
            Debug.LogError("Không thể kết bạn: " + www.downloadHandler.text);
        }
    }
    IEnumerator GetFriendRequestCount()
    {
        string url = "http://127.0.0.1:8000/get-friend-request-count/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        string token = PlayerPrefs.GetString("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            www.SetRequestHeader("Authorization", "Bearer " + token);
        }
        yield return www.SendWebRequest(); 
        if (www.result == UnityWebRequest.Result.Success)
        {
            // Nếu thành công, parse JSON và lấy số lượng yêu cầu
            string jsonResponse = www.downloadHandler.text;
            RequestCountResponse response = JsonUtility.FromJson<RequestCountResponse>(jsonResponse);
            countRequest.text = "" + response.request_count ;
        }
        else
        {
            // Nếu có lỗi, in lỗi ra console
            Debug.LogError("Error: " + www.downloadHandler.text);
        }
    }
    void Start()
    {
        StartCoroutine(GetFriendRequestCount());
    }
    [System.Serializable]
    public class RequestCountResponse
    {
        public int request_count;
    }
}
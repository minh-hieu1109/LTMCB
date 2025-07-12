using UnityEngine;
using UnityEngine.Networking;
using Mirror;
using System.Collections;

public class MatchHistoryUploader : NetworkBehaviour
{
    [Server]
    public void UploadResult(PlayerStats stats, string roomCode)
    {
        TargetUploadResult(stats.connectionToClient, stats.kills, stats.deaths, stats.coins, roomCode);
    }

    [TargetRpc]
    void TargetUploadResult(NetworkConnection target, int kills, int deaths, int moneyCollected, string roomCode)
    {
        Debug.Log("Chuẩn bị gửi dữ liệu trận đấu...");

        StartCoroutine(SendMatchHistoryCoroutine(kills, deaths, moneyCollected, roomCode));
    }

    IEnumerator SendMatchHistoryCoroutine(int kills, int deaths, int moneyCollected, string roomCode)
    {
        string url = "http://127.0.0.1:8000/matches/save_history/";

        MatchHistoryRequest request = new MatchHistoryRequest
        {
            room_code = roomCode,
            kills = kills,
            deaths = deaths,
            money_collected = moneyCollected
        };

        string jsonData = JsonUtility.ToJson(request);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Lưu lịch sử thành công!");
        }
        else
        {
            Debug.LogError("Lỗi lưu lịch sử: " + www.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class MatchHistoryRequest
    {
        public string room_code;
        public int kills;
        public int deaths;
        public int money_collected;
    }
}

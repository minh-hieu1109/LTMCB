using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UserProfileManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetProfile());
    }

    IEnumerator GetProfile()
    {
        string url = "http://127.0.0.1:8000/me/";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Lấy profile thành công: " + www.downloadHandler.text);

            ProfileResponse profile = JsonUtility.FromJson<ProfileResponse>(www.downloadHandler.text);

            PlayerPrefs.SetString("nickname", profile.nickname);
            PlayerPrefs.Save();

            Debug.Log("Đã lưu nickname vào PlayerPrefs: " + profile.nickname);
        }
        else
        {
            Debug.LogError("Lỗi lấy profile: " + www.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class ProfileResponse
    {
        public int id;
        public string nickname;
        public int score;
        public int coin;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;



public class MenuGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMPro.TMP_Text nameText; // Text để hiển thị tên người chơi
    void Start()
    {
        if (PlayerPrefs.HasKey("player_name"))
        {
            string playerName = PlayerPrefs.GetString("player_name", "Unknown");
            nameText.text = playerName;
        }
    }

    public GameObject profilePanel;
    public GameObject friendPanel;
    public GameObject logoutConfirmPanel;
    public Button btnConfirmLogout;
    public Button btnCancelLogout;

    public void OnLogoutButtonPressed()
    {
        logoutConfirmPanel.SetActive(true);

        btnConfirmLogout.onClick.RemoveAllListeners();
        btnCancelLogout.onClick.RemoveAllListeners();

        btnConfirmLogout.onClick.AddListener(() =>
        {
            logoutConfirmPanel.SetActive(false);
            PerformLogout();
        });

        btnCancelLogout.onClick.AddListener(() =>
        {
            logoutConfirmPanel.SetActive(false);
        });
    }

    void PerformLogout()
    {
        // Xóa token
        PlayerPrefs.DeleteKey("access_token");

        // Load scene đăng nhập (đặt đúng tên scene bạn đã thêm trong Build Settings)
        SceneManager.LoadScene("djangologin");
    }
    public void OpenProfilePanel()
    {
        profilePanel.SetActive(true);
        Debug.Log("OpenProfilePanel called");
    }

    

    public void OpenFriendPanel()
    {
        friendPanel.SetActive(true);
        FindPlayerManager.instance.ShowFriendListFromCache();
    }


    public void GoToLobbyScene()
    {
        SceneManager.LoadScene("LobbyScene",LoadSceneMode.Additive);
        Debug.Log("GoToLobbyScene called");
    }
    

    public void GoToSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}


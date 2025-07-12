using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


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


using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class MenuGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        Debug.Log("OpenFriendPanel called");
        
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


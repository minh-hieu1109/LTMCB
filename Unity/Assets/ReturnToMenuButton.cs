using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;

public class ReturnToMenuButton : MonoBehaviour
{
    public static ReturnToMenuButton Instance; 

    void Awake()
    {
        Instance = this;
    }

    public void OnClickReturnToMenu()
    {
        Debug.Log("Đang quay về Menu...");

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            Debug.Log("Stopping Host...");
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            Debug.Log("Stopping Client...");
            NetworkManager.singleton.StopClient();
        }

        SceneManager.LoadScene("GameScene");
    }

    public void StartAutoReturn(float delay)
    {
        StartCoroutine(ReturnAfterDelay(delay));
    }

    IEnumerator ReturnAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        SceneManager.LoadScene("GameScene");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public TMP_InputField portInput;
    public NetworkManager networkManager;

    public void OnHostClicked()
    {
        ushort port = 7777;
        if (ushort.TryParse(portInput.text, out ushort parsedPort))
            port = parsedPort;

        var telepathy = NetworkManager.singleton.transport as TelepathyTransport;
        if (telepathy != null)
        {
            telepathy.port = port;
        }
        else
        {
            Debug.LogError("Transport hiện tại không phải TelepathyTransport");
            return;
        }

        networkManager.StartHost();
        SceneManager.LoadScene("RoomScene");
    }

    public void OnJoinClicked()
    {
        ushort port = 7777;
        if (ushort.TryParse(portInput.text, out ushort parsedPort))
            port = parsedPort;

        var telepathy = NetworkManager.singleton.transport as TelepathyTransport;
        if (telepathy != null)
        {
            telepathy.port = port;
        }
        else
        {
            Debug.LogError("Transport hiện tại không phải TelepathyTransport");
            return;
        }

        networkManager.networkAddress = "localhost";
        networkManager.StartClient();
        SceneManager.LoadScene("RoomScene");
    }
}

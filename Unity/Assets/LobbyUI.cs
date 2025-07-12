using UnityEngine;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public TMP_InputField roomInput;

    public void OnHostClicked()
    {
        MatchManager.instance.CreateRoom();
    }

    public void OnJoinClicked()
    {
        string roomCode = roomInput.text.Trim();
        MatchManager.instance.JoinRoom(roomCode);
    }

    public void CloseLobbyScene()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("LobbyScene");
    }
}

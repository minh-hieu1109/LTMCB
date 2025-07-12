using Mirror;
using UnityEngine;

public class EndGameUIManager : MonoBehaviour
{
    public static EndGameUIManager Instance;

    public GameObject panel;
    public GameObject winText;
    public GameObject loseText;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        winText.SetActive(false);
        loseText.SetActive(false);
    }

    public void ShowWin()
    {
        panel.SetActive(true);
        winText.SetActive(true);
        loseText.SetActive(false);
        DisablePlayerControls();
        ReturnToMenuButton.Instance.StartAutoReturn(5f); 
    }

    public void ShowLose()
    {
        panel.SetActive(true);
        winText.SetActive(false);
        loseText.SetActive(true);
        DisablePlayerControls();
        ReturnToMenuButton.Instance.StartAutoReturn(5f);
    }

    void DisablePlayerControls()
    {
        if (NetworkClient.localPlayer != null)
        {
            var move = NetworkClient.localPlayer.GetComponent<Movement>();
            if (move != null) move.enabled = false;

            var shoot = NetworkClient.localPlayer.GetComponent<PlayerShooting>();
            if (shoot != null) shoot.enabled = false;

            var health = NetworkClient.localPlayer.GetComponent<Health>();
            if (health != null) health.enabled = false;

            var respawnManager = NetworkClient.localPlayer.GetComponent<RespawnManager>();
            if (respawnManager != null) respawnManager.enabled = false;
        }
    }
}

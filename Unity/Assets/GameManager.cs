using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Win Condition")]
    public int winScore = 5;

    void Awake()
    {
        Instance = this;
    }

    [Server]
    public void CheckWinCondition()
    {
        foreach (var player in LeaderboardManager.Instance.allPlayers)
        {
            int score = player.kills - player.deaths;
            if (score >= winScore)
            {
                // Thông báo thắng/thua
                RpcAnnounceResult(player.GetComponent<NetworkIdentity>());

                // Gửi dữ liệu lịch sử cho tất cả người chơi
                foreach (var p in LeaderboardManager.Instance.allPlayers)
                {
                    MatchHistoryUploader uploader = GetComponent<MatchHistoryUploader>();
                    uploader.UploadResult(p, PlayerPrefs.GetString("room_code"));
                }

                return;
            }
        }
    }


    [ClientRpc]
    void RpcAnnounceResult(NetworkIdentity winnerIdentity)
    {
        if (winnerIdentity.isLocalPlayer)
        {
            EndGameUIManager.Instance.ShowWin();
        }
        else
        {
            EndGameUIManager.Instance.ShowLose();
        }
    }

}

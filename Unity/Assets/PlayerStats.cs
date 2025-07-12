using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar]
    public int kills = 0;

    [SyncVar]
    public int deaths = 0;

    public void AddKill()
    {
        kills++;
    }

    public void AddDeath()
    {
        deaths++;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // ??ng ký player vào LeaderboardManager
        LeaderboardManager.Instance.RegisterPlayer(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        // Hu? ??ng ký khi player r?i tr?n
        LeaderboardManager.Instance.UnregisterPlayer(this);
    }
}

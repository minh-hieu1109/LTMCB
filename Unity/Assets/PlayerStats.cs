using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar]
    public int kills = 0;

    [SyncVar]
    public int deaths = 0;
    [SyncVar(hook = nameof(OnCoinsChanged))]
    public int coins = 0;
    public void AddKill()
    {
        kills++;
    }

    public void AddDeath()
    {
        deaths++;
    }
    [Server]
    public void AddCoin()
    {
        coins++;
    }
    void OnCoinsChanged(int oldCoins, int newCoins)
    {
        if (isLocalPlayer && CoinUIManager.Instance != null)
        {
            CoinUIManager.Instance.UpdateCoins(newCoins);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        LeaderboardManager.Instance.RegisterPlayer(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        LeaderboardManager.Instance.UnregisterPlayer(this);
    }
}

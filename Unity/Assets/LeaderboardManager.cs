using Mirror;
using System.Collections.Generic;

public class LeaderboardManager : NetworkBehaviour
{
    public static LeaderboardManager Instance;

    public List<PlayerStats> allPlayers = new List<PlayerStats>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerStats stats)
    {
        if (isServer)
        {
            allPlayers.Add(stats);
        }
    }

    public void UnregisterPlayer(PlayerStats stats)
    {
        if (isServer)
        {
            allPlayers.Remove(stats);
        }
    }
}

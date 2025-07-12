using Mirror;
using UnityEngine;

public class CoinPickup : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        var stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.AddCoin();
            NetworkServer.Destroy(gameObject);
        }
    }
}

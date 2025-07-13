using Mirror;
using UnityEngine;

public class CoinPickup : NetworkBehaviour
{
    public float lifetime = 15f;
    public override void OnStartServer()
    {
        base.OnStartServer();
        Invoke(nameof(DestroySelf), lifetime);
    }
    [Server]
    void DestroySelf()
    {
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }
    [ServerCallback]
    void OnCollisionEnter(Collision collision)
    {
        var stats = collision.collider.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.AddCoin();
            NetworkServer.Destroy(gameObject);
        }
    }
}

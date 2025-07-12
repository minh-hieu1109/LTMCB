using Mirror;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    public GameObject coinPrefab;
    public int coinCount = 10;

    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
            GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(coin);
        }
    }
}

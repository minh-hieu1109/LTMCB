using Mirror;
using UnityEngine;
using System.Collections;

public class CoinSpawner : NetworkBehaviour
{
    public GameObject coinPrefab;
    public int coinsPerSpawn = 10;
    public float spawnInterval = 1f; 
    public float spawnHeight = 20f;  

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(SpawnCoinsContinuously());
    }

    IEnumerator SpawnCoinsContinuously()
    {
        while (true)
        {
            for (int i = 0; i < coinsPerSpawn; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-100f, 100f),
                    spawnHeight,
                    Random.Range(-100f, 100f)
                );

                GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);

                

                NetworkServer.Spawn(coin);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}

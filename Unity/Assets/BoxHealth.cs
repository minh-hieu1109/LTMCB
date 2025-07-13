using Mirror;
using UnityEngine;

public class BoxHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;

    [SerializeField] private GameObject deathVFX;
    public GameObject goldPrefab;
    public int goldAmount = 5;

    public void TakeDamage(int damage)
    {
        if (health <= 0) return;

        health -= damage;

        if (health <= 0)
        {
            RpcPlayDeathVFX(transform.position);

            // Server spawn vàng
            SpawnGoldOnServer(transform.position);

            StartCoroutine(DestroyDelay());
        }
    }

    [ClientRpc]
    void RpcPlayDeathVFX(Vector3 position)
    {
        if (deathVFX != null)
        {
            var vfx = Instantiate(deathVFX, position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    [Server]
    void SpawnGoldOnServer(Vector3 position)
    {
        if (goldPrefab == null) return;

        for (int i = 0; i < goldAmount; i++)
        {
            Vector3 spawnPos = position + Random.insideUnitSphere * 0.5f;
            Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject gold = Instantiate(goldPrefab, spawnPos, rot);

            Rigidbody rb = gold.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDir = Random.onUnitSphere + Vector3.up * 1f;
                rb.AddForce(forceDir * Random.Range(3f, 8f), ForceMode.Impulse);
            }

            NetworkServer.Spawn(gold);
        }
    }

    private System.Collections.IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(0.1f);
        NetworkServer.Destroy(gameObject);
    }
}

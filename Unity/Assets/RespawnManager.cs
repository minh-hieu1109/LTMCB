using UnityEngine;
using Mirror;

public class RespawnManager : NetworkBehaviour
{
    public static RespawnManager Instance;

    public Transform[] spawnPoints;

    void Awake()
    {
        Instance = this;
    }

    [Server]
    public void Respawn(GameObject player)
    {
        NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
        if (identity == null || identity.connectionToClient == null)
        {
            Debug.LogWarning("Respawn failed: no valid connectionToClient");
            return;
        }

        StartCoroutine(RespawnCoroutine(player, identity.connectionToClient));
    }

    private System.Collections.IEnumerator RespawnCoroutine(GameObject player, NetworkConnectionToClient conn)
    {
        SetPlayerVisible(player, false);

        yield return new WaitForSeconds(2f);

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Vector3 safeSpawn = spawnPoint.position + Vector3.up * 0.5f;
        player.transform.SetPositionAndRotation(safeSpawn, spawnPoint.rotation);

        if (rb != null)
        {
            rb.isKinematic = false; 
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero; 
        }

        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
            health.SetInvincible(3f);
        }

        SetPlayerVisible(player, true);
        RpcNotifyRespawn(player);
        TargetOnRespawn(conn);
    }



    void SetPlayerVisible(GameObject player, bool visible)
    {
        var renderers = player.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = visible;

        var colliders = player.GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = visible;
    }


    [TargetRpc]
    void TargetOnRespawn(NetworkConnection target)
    {
        Debug.Log("Respawned by server");
    }
    [ClientRpc]
    void RpcNotifyRespawn(GameObject player)
    {
        if (player == null) return;

        var deathHandler = player.GetComponent<death>();
        if (deathHandler != null)
        {
            deathHandler.OnRespawn();
        }

        var renderers = player.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = true;

        var colliders = player.GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = true;

        // Bật movement lại
        var move = player.GetComponent<Movement>();
        if (move != null)
            move.enabled = true;
    }
}

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
        StartCoroutine(RespawnCoroutine(player));
    }

    private System.Collections.IEnumerator RespawnCoroutine(GameObject player)
    {
        SetPlayerVisible(player, false);

        yield return new WaitForSeconds(2f);

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
            health.SetInvincible(3f);
        }

        SetPlayerVisible(player, true);

        TargetOnRespawn(player.GetComponent<NetworkIdentity>().connectionToClient);
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
}

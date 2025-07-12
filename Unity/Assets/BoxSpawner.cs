using UnityEngine;
using Mirror;

public class BoxSpawner : NetworkBehaviour
{
    public GameObject boxPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnOneBox();
    }

    [Server]
    void SpawnOneBox()
    {
        Vector3 spawnPos = new Vector3(0f, 0f, 0f); // Vị trí trung tâm

        GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(box);
    }
}

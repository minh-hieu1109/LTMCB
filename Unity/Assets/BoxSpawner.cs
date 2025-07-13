using UnityEngine;
using Mirror;
using System.Collections;

public class BoxSpawner : NetworkBehaviour
{
    public GameObject boxPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(SpawnBoxesLoop());
    }

    IEnumerator SpawnBoxesLoop()
    {
        while (true)
        {
            SpawnOneBox();
            yield return new WaitForSeconds(5f); // đợi 3 giây
        }
    }

    [Server]
    void SpawnOneBox()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(-30f, 30f),
            1f,
            Random.Range(-30f, 30f)
        );

        GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);

        // Random loại Box
        BoxType randomType = (BoxType)Random.Range(0, System.Enum.GetValues(typeof(BoxType)).Length);

        // Gán loại Box
        BoxPickup pickup = box.GetComponent<BoxPickup>();
        if (pickup != null)
        {
            pickup.boxType = randomType;
        }

        NetworkServer.Spawn(box);
    }
}

using Mirror;
using UnityEngine;

public class death : NetworkBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera deathCamera;

    private bool isDead = false;

    void Update()
    {
        if (!isLocalPlayer) return;

        if (isDead && Input.GetKeyDown(KeyCode.P))
        {
            CmdRequestRespawn();
        }
    }

    public void OnDeath()
    {
        if (!isLocalPlayer) return;

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (deathCamera != null)
            deathCamera.enabled = true;

        isDead = true;
    }

    public void OnRespawn()
    {
        if (!isLocalPlayer) return;

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (deathCamera != null)
            deathCamera.enabled = false;

        isDead = false;
    }

    [Command]
    void CmdRequestRespawn()
    {
        RespawnManager.Instance.Respawn(gameObject);
    }
}
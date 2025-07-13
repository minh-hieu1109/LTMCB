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

    void OnGUI()
    {
        if (!isLocalPlayer) return;

        if (isDead)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 50;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            Rect rect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 25, 400, 50);
            GUI.Label(rect, "Press P to revive", style);
        }
    }

    [Command]
    void CmdRequestRespawn()
    {
        RespawnManager.Instance.Respawn(gameObject);
    }
}

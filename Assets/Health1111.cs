using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
public class Health : MonoBehaviourPun
{
    public int health;
    public bool isLocalPlayer;
    [Header("VFX")]
    public GameObject deathVFX;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    private bool isInvincible = false;
    private Coroutine invincibleCoroutine;
    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;
        health -= damage;

        if (photonView.IsMine)
        {
            healthText.text = health.ToString();
        }

        if (health <= 0)
        {
            if (photonView.IsMine)
            {
                photonView.RPC("PlayDeathVFX", RpcTarget.All, transform.position);

                PhotonNetwork.Destroy(gameObject);
                RoomManager.instance.SpawnPlayer();
                RoomManager.instance.deaths++;
                RoomManager.instance.SetHashes();
            }
        }
    }
    [PunRPC]
    public void PlayDeathVFX(Vector3 position)
    {
        if (deathVFX != null)
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.up);

            GameObject vfx = Instantiate(deathVFX, position, rotation);
            Destroy(vfx, 2f);
        }
    }

    public void SetInvincible(float duration)
    {
        if (invincibleCoroutine != null)
            StopCoroutine(invincibleCoroutine);

        invincibleCoroutine = StartCoroutine(InvincibleCoroutine(duration));
    }
    public void CancelInvincible()
    {
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }
        isInvincible = false;
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

}
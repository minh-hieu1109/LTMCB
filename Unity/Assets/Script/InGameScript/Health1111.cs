using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;

public class Health : NetworkBehaviour
{
    [Header("Stats")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 100;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private GameObject invincibleVFXObject;

    private bool isInvincible = false;
    private Coroutine invincibleCoroutine;

    #region Damage / Heal
    [Server]
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        health -= damage;
        if (health <= 0)
        {
            RpcPlayDeathVFX(transform.position);

            RpcNotifyDeath();
        }
    }
    [ClientRpc]
    void RpcNotifyDeath()
    {
        var deathHandler = GetComponent<death>();
        if (deathHandler != null)
        {
            deathHandler.OnDeath();
        }

        // Tắt renderer và collider
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = false;

        // Tắt movement
        var move = GetComponent<Movement>();
        if (move != null)
            move.enabled = false;
    }

    void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (isLocalPlayer && healthText != null)
        {
            healthText.text = newHealth.ToString();
        }

        if (isLocalPlayer && HealthUIManager.Instance != null)
        {
            HealthUIManager.Instance.UpdateHealth(newHealth);
        }
    }
    #endregion

    #region Death VFX
    [ClientRpc]
    void RpcPlayDeathVFX(Vector3 position)
    {
        if (deathVFX != null)
        {
            var vfx = Instantiate(deathVFX, position, Quaternion.LookRotation(Vector3.up));
            Destroy(vfx, 2f);
        }
    }
    #endregion

    #region Invincible

    public void SetInvincible(float duration)
    {
        if (invincibleCoroutine != null)
            StopCoroutine(invincibleCoroutine);

        invincibleCoroutine = StartCoroutine(InvincibleCoroutine(duration));
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        isInvincible = true;
        RpcEnableInvincibleVFX();

        yield return new WaitForSeconds(duration);
        if (isInvincible)
        {
            isInvincible = false;
            RpcDisableInvincibleVFX();
        }

        invincibleCoroutine = null;
    }

    [Server]
    void CancelInvincible()
    {
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }

        isInvincible = false;
        RpcDisableInvincibleVFX();
    }

    [Command]
    void CmdCancelInvincible()
    {
        CancelInvincible();
    }

    [ClientRpc]
    void RpcEnableInvincibleVFX()
    {
        if (invincibleVFXObject != null)
            invincibleVFXObject.SetActive(true);
    }

    [ClientRpc]
    void RpcDisableInvincibleVFX()
    {
        if (invincibleVFXObject != null)
            invincibleVFXObject.SetActive(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (isInvincible && Input.GetMouseButtonDown(0))
        {
            CmdCancelInvincible();
        }
    }

    #endregion


    public bool IsInvincible() => isInvincible;

    public void ResetHealth()
    {
        health = 100;
        if (isLocalPlayer && healthText != null)
        {
            healthText.text = health.ToString();
            HealthUIManager.Instance.SetMaxHealth(100);
            HealthUIManager.Instance.UpdateHealth(health);
        }
    }

    void Start()
    {
        if (isLocalPlayer && healthText != null)
        {
            healthText.text = health.ToString();
        }

        if (isLocalPlayer && HealthUIManager.Instance != null)
        {
            HealthUIManager.Instance.SetMaxHealth(100);
            HealthUIManager.Instance.UpdateHealth(health);
        }
    }
}

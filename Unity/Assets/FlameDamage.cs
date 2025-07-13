using UnityEngine;
using Mirror;

public class FlameDamage : NetworkBehaviour
{
    public int damagePerTick = 999;
    public float tickInterval = 1f;

    private float nextDamageTime;
    public GameObject attacker;

    public void SetAttacker(GameObject attackerObj)
    {
        attacker = attackerObj;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!isServer) return;
        BoxHealth boxHealth = other.gameObject.GetComponent<BoxHealth>();
        if (boxHealth != null)
        {
            boxHealth.TakeDamage(damagePerTick);
        }
        Health targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth != null && Time.time >= nextDamageTime)
        {
            Debug.Log($"[FlameDamage] Damaging {targetHealth.gameObject.name}, attacker = {attacker}");
            targetHealth.TakeDamage(damagePerTick, attacker);
            nextDamageTime = Time.time + tickInterval;
        }
    }
}

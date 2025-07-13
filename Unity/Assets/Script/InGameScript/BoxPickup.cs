using UnityEngine;
using Mirror;

public class BoxPickup : NetworkBehaviour
{
    public BoxType boxType;
    public int healAmount = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        NetworkIdentity identity = other.GetComponent<NetworkIdentity>();
        if (identity != null && identity.connectionToClient != null)
        {
            var health = other.GetComponent<Health>();
            var movement = other.GetComponent<Movement>();

            switch (boxType)
            {
                case BoxType.FlameThrower:
                    if (movement != null)
                    {
                        movement.EnableFlameThrowerAbility(3f);
                        TargetShowPickup(identity.connectionToClient, "Bạn đã nhặt vũ khí phun lửa! Ấn E để phun trong 10 giây.");
                    }
                    break;

                case BoxType.HealthAndSpeed:
                    if (health != null)
                    {
                        health.Heal(healAmount);
                    }
                    if (movement != null)
                    {
                        movement.EnableSpeedBoost(2f, 3f);
                    }
                    TargetShowPickup(identity.connectionToClient, "Bạn đã nhặt hộp hồi máu và tăng tốc 10 giây!");
                    break;
            }

            NetworkServer.Destroy(gameObject);
        }
    }

    [TargetRpc]
    void TargetShowPickup(NetworkConnection target, string message)
    {
        Debug.Log(message);
    }
}

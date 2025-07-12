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
                //case BoxType.Heal:
                //    if (health != null)
                //    {
                //        health.Heal(healAmount);
                //        TargetShowPickup(identity.connectionToClient, "Bạn đã nhặt hộp hồi máu!");
                //    }
                //    break;

                case BoxType.FlameThrower:
                    if (movement != null)
                    {
                        movement.EnableFlameThrowerAbility();
                        TargetShowPickup(identity.connectionToClient, "Bạn đã nhặt vũ khí phun lửa! Ấn E để phun.");
                    }
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

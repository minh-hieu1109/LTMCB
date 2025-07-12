using Mirror;
using UnityEngine;

public class FollowTarget : NetworkBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.zero;

    void Update()
    {

        if (target != null)
        {
            transform.position = target.position + target.TransformDirection(offset);
            transform.rotation = target.rotation;
        }
    }
}

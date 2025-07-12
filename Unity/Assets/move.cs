using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 120.0f;

    [Header("Wheels")]
    public GameObject[] leftWheels;
    public GameObject[] rightWheels;
    public float wheelRotationSpeed = 200.0f;

    private Rigidbody rb;

    private float moveInput;
    private float rotationInput;
    [SyncVar]
    private bool canFire = false;

    public GameObject flameThrowerPrefab; 
    private GameObject activeFlame;
    public Transform firePoint;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        moveInput = Input.GetAxis("Vertical");
        rotationInput = Input.GetAxis("Horizontal");

        CmdMove(moveInput, rotationInput);

        RotateWheels(moveInput, rotationInput);

        if (canFire && Input.GetKeyDown(KeyCode.E))
        {
            CmdToggleFlame();
        }
    }

    [Command]
    void CmdMove(float move, float rotate)
    {
        MoveTankObj(move);
        RotateTank(rotate);
    }

    void MoveTankObj(float input)
    {
        Vector3 moveDirection = transform.forward * input * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    void RotateTank(float input)
    {
        float rotation = input * rotationSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void RotateWheels(float moveInput, float rotationInput)
    {
        float wheelRotation = moveInput * wheelRotationSpeed * Time.deltaTime;

        foreach (GameObject wheel in leftWheels)
        {
            wheel.transform.Rotate(wheelRotation - rotationInput * wheelRotationSpeed * Time.deltaTime,0.0f,0.0f);
            
        }

        foreach (GameObject wheel in rightWheels)
        {
            wheel.transform.Rotate(wheelRotation + rotationInput * wheelRotationSpeed * Time.deltaTime,0.0f,0.0f);
        }
    }
    [Server]
    public void EnableFlameThrowerAbility()
    {
        canFire = true;
    }
    [Command]
    void CmdToggleFlame()
    {
        if (activeFlame == null)
        {
            GameObject flameInstance = Instantiate(flameThrowerPrefab, firePoint.position, firePoint.rotation);
            NetworkServer.Spawn(flameInstance);

            FollowTarget follow = flameInstance.AddComponent<FollowTarget>();
            follow.target = firePoint;
            follow.offset = Vector3.zero;

            // Tìm FlameDamage trong con
            FlameDamage flameDamage = flameInstance.GetComponentInChildren<FlameDamage>();
            if (flameDamage != null)
            {
                flameDamage.SetAttacker(gameObject);
            }

            activeFlame = flameInstance;
        }
        else
        {
            NetworkServer.Destroy(activeFlame);
            activeFlame = null;
        }
    }




}
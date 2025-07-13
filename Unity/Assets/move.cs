using UnityEngine;
using Mirror;
using System.Collections;

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

    [Header("Flame Thrower")]
    public GameObject flameThrowerPrefab;
    public Transform firePoint;

    private Rigidbody rb;

    private float moveInput;
    private float rotationInput;

    [SyncVar]
    private bool canFire = false;

    private GameObject activeFlame;

    private Coroutine speedBoostCoroutine;
    private Coroutine flameBuffCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        HandleInput();
        RotateWheels(moveInput, rotationInput);
    }

    void HandleInput()
    {
        moveInput = Input.GetAxis("Vertical");
        rotationInput = Input.GetAxis("Horizontal");

        CmdMove(moveInput, rotationInput);

        if (canFire && Input.GetKeyDown(KeyCode.E))
        {
            CmdToggleFlame();
        }
    }

    [Command]
    void CmdMove(float move, float rotate)
    {
        ApplyMovement(move, rotate);
    }

    void ApplyMovement(float move, float rotate)
    {
        Vector3 moveVector = transform.forward * move * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveVector);

        float rotationAngle = rotate * rotationSpeed * Time.deltaTime;
        Quaternion rotationQuat = Quaternion.Euler(0, rotationAngle, 0);
        rb.MoveRotation(rb.rotation * rotationQuat);
    }

    void RotateWheels(float move, float rotate)
    {
        float baseRot = move * wheelRotationSpeed * Time.deltaTime;
        float rotAdjust = rotate * wheelRotationSpeed * Time.deltaTime;

        foreach (GameObject wheel in leftWheels)
        {
            wheel.transform.Rotate(baseRot - rotAdjust, 0, 0);
        }

        foreach (GameObject wheel in rightWheels)
        {
            wheel.transform.Rotate(baseRot + rotAdjust, 0, 0);
        }
    }

    [Server]
    public void EnableSpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null)
            return; // Buff đã có, không cộng dồn

        speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        moveSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed /= multiplier;
        speedBoostCoroutine = null;
    }

    [Server]
    public void EnableFlameThrowerAbility(float duration = 10f)
    {
        if (flameBuffCoroutine != null)
            return; // Buff đã có, không cộng dồn

        flameBuffCoroutine = StartCoroutine(FlameThrowerBuffRoutine(duration));
    }

    private IEnumerator FlameThrowerBuffRoutine(float duration)
    {
        canFire = true;
        yield return new WaitForSeconds(duration);
        canFire = false;

        // Nếu đang bật lửa thì tắt luôn
        if (activeFlame != null)
        {
            NetworkServer.Destroy(activeFlame);
            activeFlame = null;
        }

        flameBuffCoroutine = null;
    }

    [Command]
    void CmdToggleFlame()
    {
        if (activeFlame == null)
        {
            GameObject flameObj = Instantiate(flameThrowerPrefab, firePoint.position, firePoint.rotation);
            NetworkServer.Spawn(flameObj);

            FollowTarget follow = flameObj.AddComponent<FollowTarget>();
            follow.target = firePoint;
            follow.offset = Vector3.zero;

            FlameDamage flameDamage = flameObj.GetComponentInChildren<FlameDamage>();
            if (flameDamage != null)
            {
                flameDamage.SetAttacker(gameObject);
            }

            activeFlame = flameObj;
        }
        else
        {
            NetworkServer.Destroy(activeFlame);
            activeFlame = null;
        }
    }
}

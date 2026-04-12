using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveController : MonoBehaviour
{
    public float moveSpeed = 5f;

    PlayerMove inputs;
    Animator animator;
    Rigidbody body;
    Camera mainCamera;

    private Vector3 moveDirection;
    private Vector3 moveVelocity;

    private void Awake()
    {
        inputs = new PlayerMove();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (body != null)
        {
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
    }

    private void OnDisable()
    {
        inputs.Player.Disable();
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        Vector2 moveInput = inputs.Player.Move.ReadValue<Vector2>();

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 worldMoveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;
        moveDirection = worldMoveDirection.normalized;
        moveVelocity = moveDirection * moveSpeed;

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void FixedUpdate()
    {
        if (body != null && moveVelocity != null)
        {
            Vector3 newVelocity = new Vector3(moveVelocity.x, 0, moveVelocity.z);
            body.velocity = newVelocity;
        }
    }
}

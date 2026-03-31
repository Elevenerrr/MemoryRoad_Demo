using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    [Header("Rotation Settings - Mouse Move")]
    public bool useMouseMoveRotation = true;
    //public bool requireKeyForMouseRotation = false;
    //public KeyCode mouseMoveKey = KeyCode.W;
    public float mouseMoveXSensitivity = 2f;
    public float mouseMoveYSensitivity = 2f;

    [Header("Rotation Settings - Right Click")]
    public bool useRightClickRotation = true;
    public float rightClickXSpeed = 200f;
    public float rightClickYSpeed = 200f;

    [Header("Vertical Limits")]
    public float yMinLimit = -50f;
    public float yMaxLimit = 50f;

    [Header("Zoom Settings")]
    public float distance = 10f;
    public float minDistance = 2f;
    public float maxDistance = 30f;
    public float zoomSpeed = 10f;

    [Header("Damping")]
    public bool needDamping = true;
    public float damping = 5f;

    [Header("Cursor Settings")]
    public bool lockCursor = true;

    private float x = 0f;
    private float y = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target != null)
        {
            distance = Vector3.Distance(transform.position, target.position);
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        bool isRotating = false;

        if (useRightClickRotation && Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * rightClickXSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * rightClickYSpeed * 0.02f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            isRotating = true;
        }

        if (useMouseMoveRotation)
        {
            //bool canRotate = !requireKeyForMouseRotation || Input.GetKey(mouseMoveKey);

            //if (canRotate)
            //{
            //    float mouseX = Input.GetAxis("Mouse X");
            //    float mouseY = Input.GetAxis("Mouse Y");

            //    if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
            //    {
            //        x += mouseX * mouseMoveXSensitivity;
            //        y -= mouseY * mouseMoveYSensitivity;
            //        y = ClampAngle(y, yMinLimit, yMaxLimit);
            //        isRotating = true;
            //    }
            //}
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
            {
                x += mouseX * mouseMoveXSensitivity;
                y -= mouseY * mouseMoveYSensitivity;
                y = ClampAngle(y, yMinLimit, yMaxLimit);
                isRotating = true;
            }
        }

        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(y, x, 0f);
        Vector3 disVector = new Vector3(0f, 0f, -distance);
        Vector3 desiredPosition = rotation * disVector + target.position;

        if (needDamping)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * damping * (isRotating ? 2f : 1f));
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * damping * (isRotating ? 2f : 1f));
        }
        else
        {
            transform.rotation = rotation;
            transform.position = desiredPosition;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    public Vector3 GetCameraForward()
    {
        return transform.forward;
    }

    public Vector3 GetCameraRight()
    {
        return transform.right;
    }
}

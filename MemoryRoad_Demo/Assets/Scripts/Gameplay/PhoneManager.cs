using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance { get; private set; }

    [Header("Settings")]
    public float autoLockTime = 5f;
    public bool useLayerFilter = false;
    public LayerMask interactableLayer;
    public float interactionDistance = 10f;

    [Header("Phone State")]
    public bool isPhoneEquipped = false;
    public bool isPhoneActive = false;
    public float lastInteractionTime;

    [Header("Photos")]
    public List<string> takenPhotos = new List<string>();
    public int maxPhotos = 10;

    [Header("Current Target")]
    public InteractableObject currentTarget;

    [Header("Events")]
    public UnityEvent OnPhoneEquipped;
    public UnityEvent OnPhoneUnequipped;
    public UnityEvent OnPhoneActivated;
    public UnityEvent OnPhoneDeactivated;
    public UnityEvent OnPhotoTaken;
    public UnityEvent OnRecordingPlayed;

    private Camera mainCamera;
    private PhoneHUD phoneHUD;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        phoneHUD = FindObjectOfType<PhoneHUD>();
        lastInteractionTime = Time.time;
        Debug.Log("[PhoneManager] 已启动");
    }

    void Update()
    {
        HandleInput();
        CheckAutoLock();
        UpdateCurrentTarget();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePhoneEquip();
            return;
        }

        if (!isPhoneActive) return;

        lastInteractionTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseCameraFunction();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseAudioFunction();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseOverlayFunction();
        }
    }

    void CheckAutoLock()
    {
        if (!isPhoneActive) return;

        float idleTime = Time.time - lastInteractionTime;
        if (idleTime > autoLockTime)
        {
            Debug.Log($"[PhoneManager] 息屏了！闲置时间: {idleTime:F1}秒");
            DeactivatePhone();
        }
    }

    void TogglePhoneEquip()
    {
        isPhoneEquipped = !isPhoneEquipped;
        Debug.Log($"[PhoneManager] 手机装备状态: {isPhoneEquipped}");

        if (isPhoneEquipped)
        {
            OnPhoneEquipped?.Invoke();
            ActivatePhone();
        }
        else
        {
            OnPhoneUnequipped?.Invoke();
            DeactivatePhone();
        }
    }

    void ActivatePhone()
    {
        isPhoneActive = true;
        lastInteractionTime = Time.time;
        OnPhoneActivated?.Invoke();
        Debug.Log("[PhoneManager] 手机已激活");
    }

    void DeactivatePhone()
    {
        isPhoneActive = false;
        currentTarget = null;
        OnPhoneDeactivated?.Invoke();
        Debug.Log("[PhoneManager] 手机已息屏");
    }

    void UpdateCurrentTarget()
    {
        if (!isPhoneActive) return;

        RaycastHit hit;
        if (TryGetInteractableObject(out hit))
        {
            currentTarget = hit.collider.GetComponent<InteractableObject>();
        }
        else
        {
            currentTarget = null;
        }
    }

    public void UseCameraFunction()
    {
        if (currentTarget == null)
        {
            Debug.Log("[手机] 拍摄失败：没有对准任何物体");
            return;
        }

        if (currentTarget.interactType == InteractableType.Photo)
        {
            string photoId = currentTarget.objectId;
            if (!takenPhotos.Contains(photoId) && takenPhotos.Count < maxPhotos)
            {
                takenPhotos.Add(photoId);
                Debug.Log($"[手机] 拍照成功: {photoId}");
                currentTarget.OnInteract();
                OnPhotoTaken?.Invoke();
            }
            else
            {
                Debug.Log("[手机] 已拍过此照片或相册已满");
            }
        }
        else if (currentTarget.interactType == InteractableType.Vision)
        {
            Debug.Log($"[手机] 触发穿墙: {currentTarget.objectId}");
            currentTarget.OnInteract();
            OnPhotoTaken?.Invoke();
        }
        else
        {
            Debug.Log("[手机] 这个物体不能拍摄");
        }
    }

    public void UseAudioFunction()
    {
        if (currentTarget == null)
        {
            Debug.Log("[手机] 播放失败：没有对准任何物体");
            return;
        }

        if (currentTarget.interactType == InteractableType.Audio)
        {
            currentTarget.OnInteract();
            Debug.Log($"[手机] 播放录音: {currentTarget.objectId}");
            OnRecordingPlayed?.Invoke();
        }
        else
        {
            Debug.Log("[手机] 这个物体不能播放录音");
        }
    }

    public void UseOverlayFunction()
    {
        if (currentTarget == null)
        {
            Debug.Log("[手机] 覆盖失败：没有对准任何物体");
            return;
        }

        if (currentTarget.interactType == InteractableType.Overlay)
        {
            currentTarget.OnInteract();
            Debug.Log($"[手机] 触发覆盖选择: {currentTarget.objectId}");
        }
        else
        {
            Debug.Log("[手机] 这个物体不能覆盖");
        }
    }

    bool TryGetInteractableObject(out RaycastHit hit)
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (useLayerFilter)
        {
            return Physics.Raycast(ray, out hit, interactionDistance, interactableLayer);
        }
        else
        {
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                return hit.collider.GetComponent<InteractableObject>() != null;
            }
            return false;
        }
    }

    public bool HasPhoto(string photoId)
    {
        return takenPhotos.Contains(photoId);
    }

    public int GetPhotoCount()
    {
        return takenPhotos.Count;
    }
}

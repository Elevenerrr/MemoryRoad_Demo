using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance { get; private set; }

    [Header("Settings")]
    public float autoLockTime = 5f;
    public float interactionDistance = 3f;

    [Header("Phone State")]
    public bool isPhoneEquipped = false;
    public bool isPhoneActive = false;
    public bool isInventoryOpen = false;
    public bool isDialogueOpen = false;
    public float lastInteractionTime;

    [Header("Current Target")]
    public InteractableObject currentTarget;
    public List<InteractableObject> nearbyObjects = new List<InteractableObject>();

    [Header("Events")]
    public UnityEvent OnPhoneEquipped;
    public UnityEvent OnPhoneUnequipped;
    public UnityEvent OnPhoneActivated;
    public UnityEvent OnPhoneDeactivated;
    public UnityEvent OnPhotoTaken;
    public UnityEvent OnAudioPlayed;

    private Transform playerTransform;
    private PhoneHUD phoneHUD;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        phoneHUD = FindObjectOfType<PhoneHUD>();
        lastInteractionTime = Time.time;
        Debug.Log("[PhoneManager] 已启动");
    }

    void Update()
    {
        if (isInventoryOpen || isDialogueOpen) return;

        HandleInput();
        CheckAutoLock();
        UpdateNearbyObjects();
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
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseAudioFunction();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
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

    void UpdateNearbyObjects()
    {
        if (playerTransform == null) return;

        InteractableObject[] allObjects = FindObjectsOfType<InteractableObject>();
        nearbyObjects.Clear();
        currentTarget = null;

        float closestDistance = interactionDistance;

        foreach (var obj in allObjects)
        {
            float distance = Vector3.Distance(playerTransform.position, obj.transform.position);
            if (distance <= obj.interactionRange)
            {
                nearbyObjects.Add(obj);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = obj;
                }
            }
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

    public void OpenInventory()
    {
        isInventoryOpen = true;
        Debug.Log("[PhoneManager] 打开背包");
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        Debug.Log("[PhoneManager] 关闭背包");
    }

    public void OpenDialogue()
    {
        isDialogueOpen = true;
        Debug.Log("[PhoneManager] 对话开启");
    }

    public void CloseDialogue()
    {
        isDialogueOpen = false;
        Debug.Log("[PhoneManager] 对话结束");
    }

    public void UseCameraFunction()
    {
        if (currentTarget == null)
        {
            Debug.Log("[手机] 拍摄失败：附近没有可交互物体");
            return;
        }

        if (currentTarget.interactType == InteractableType.Photo)
        {
            string photoId = currentTarget.objectId;

            if (PhotoManager.Instance != null)
            {
                bool success = PhotoManager.Instance.AddPhoto(photoId);
                if (success)
                {
                    Debug.Log($"[手机] 拍照成功: {photoId}");
                    currentTarget.OnInteract();
                    OnPhotoTaken?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("[手机] PhotoManager 未找到！");
            }
        }
        else if (currentTarget.interactType == InteractableType.Vision)
        {
            Debug.Log($"[手机] 触发闪光穿墙: {currentTarget.objectId}");
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
            Debug.Log("[手机] 播放失败：附近没有可交互物体");
            return;
        }

        if (currentTarget.interactType == InteractableType.Audio)
        {
            currentTarget.OnInteract();
            Debug.Log($"[手机] 播放录音: {currentTarget.objectId}");
            OnAudioPlayed?.Invoke();
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
            Debug.Log("[手机] 覆盖失败：附近没有可交互物体");
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

    public int GetPhotoCount()
    {
        if (PhotoManager.Instance != null)
            return PhotoManager.Instance.GetPhotoCount();
        return 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance { get; private set; }

    [Header("手机是否已解锁（交互墓碑后为 true，跨场景持久）")]
    public static bool isPhoneUnlocked = false;

    [Header("Settings")]
    public float autoLockTime = 5f;
    public float interactionDistance = 3f;

    [Header("Idle Timeout Settings")]
    public float idleTimeout = 10f;
    public bool idleTimeoutEnabled = true;

    [Header("Phone State")]
    public bool isPhoneEquipped = false;
    public bool isPhoneActive = false;
    public bool isInventoryOpen = false;
    public bool isDialogueOpen = false;
    public float lastInteractionTime;
    private float lastIdleCheckTime;
    private bool isIdleTimerPaused = false;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        phoneHUD = FindObjectOfType<PhoneHUD>();
        lastInteractionTime = Time.time;
        lastIdleCheckTime = Time.time;
        Debug.Log("[PhoneManager] 已启动");
    }

    void Update()
    {
        if (isInventoryOpen || isDialogueOpen)
        {
            return;
        }

        HandleInput();
        CheckAutoLock();
        CheckIdleTimeout();
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

    void CheckIdleTimeout()
    {
        if (!idleTimeoutEnabled || !isPhoneEquipped || isIdleTimerPaused) return;

        float idleTime = Time.time - lastIdleCheckTime;
        if (idleTime > idleTimeout)
        {
            Debug.Log($"[PhoneManager] 闲置超时 ({idleTimeout:F1}秒)，退出手机模式！");
            ForceUnequipPhone();
        }
    }

    void ResetIdleTimer()
    {
        lastIdleCheckTime = Time.time;
        lastInteractionTime = Time.time;
    }

    void ForceUnequipPhone()
    {
        isPhoneEquipped = false;
        isPhoneActive = false;
        isIdleTimerPaused = false;
        currentTarget = null;
        OnPhoneUnequipped?.Invoke();
        OnPhoneDeactivated?.Invoke();
        Debug.Log("[PhoneManager] 因闲置超时，强制退出手机模式");
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
        // 检查手机是否已解锁
        if (!isPhoneUnlocked)
            return;

        if (isPhoneEquipped && isPhoneActive)
        {
            isPhoneEquipped = false;
            OnPhoneUnequipped?.Invoke();
            DeactivatePhone();
            Debug.Log("[PhoneManager] 卸下手机");
        }
        else if (isPhoneEquipped && !isPhoneActive)
        {
            ActivatePhone();
            Debug.Log("[PhoneManager] 重新激活手机");
        }
        else
        {
            isPhoneEquipped = true;
            OnPhoneEquipped?.Invoke();
            ActivatePhone();
            Debug.Log("[PhoneManager] 装备手机");
        }
    }

    void ActivatePhone()
    {
        isPhoneActive = true;
        lastInteractionTime = Time.time;
        lastIdleCheckTime = Time.time;
        isIdleTimerPaused = false;
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
        isIdleTimerPaused = true;
        Debug.Log("[PhoneManager] 打开背包");
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        isIdleTimerPaused = false;
        ResetIdleTimer();
        Debug.Log("[PhoneManager] 关闭背包");
    }

    public void OpenDialogue()
    {
        isDialogueOpen = true;
        isIdleTimerPaused = true;
        Debug.Log("[PhoneManager] 对话开启");
    }

    public void CloseDialogue()
    {
        isDialogueOpen = false;
        isIdleTimerPaused = false;
        ResetIdleTimer();
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
                    ResetIdleTimer();
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
            ResetIdleTimer();
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
            ResetIdleTimer();
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
            ResetIdleTimer();
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

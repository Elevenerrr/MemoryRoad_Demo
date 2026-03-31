using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum InteractableType
{
    None,
    Photo,       // 标识1: 拍照
    Vision,      // 标识2: 视觉穿墙
    Audio,       // 标识3: 录音驱赶
    Overlay      // 标识4: 覆盖选择
}

public class InteractableObject : MonoBehaviour
{
    [Header("Basic Info")]
    public string objectId;
    public InteractableType interactType = InteractableType.None;
    public string displayName;

    [Header("Interaction Settings")]
    public bool canInteract = true;
    public float interactionCooldown = 0.5f;
    private float lastInteractTime;

    [Header("Events")]
    public UnityEvent OnInteracted;

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = gameObject.name;
        }
    }

    public virtual void OnInteract()
    {
        if (!canInteract) return;

        if (Time.time - lastInteractTime < interactionCooldown) return;

        lastInteractTime = Time.time;
        OnInteracted?.Invoke();

        Debug.Log($"[交互] {displayName} ({interactType}) 被触发");
    }

    public virtual void OnPlayerEnterRange()
    {
        Debug.Log($"[交互提示] 靠近 {displayName}");
    }

    public virtual void OnPlayerExitRange()
    {
        Debug.Log($"[交互提示] 离开 {displayName}");
    }
}

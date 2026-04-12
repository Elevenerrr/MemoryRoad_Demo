using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum InteractableType
{
    None,
    Photo,
    Vision,
    Audio,
    Overlay
}

public class InteractableObject : MonoBehaviour
{
    [Header("Basic Info")]
    public string objectId;
    public InteractableType interactType = InteractableType.None;
    public string displayName;

    [Header("Interaction Range")]
    public float interactionRange = 3f;
    public bool showRangeIndicator = true;

    [Header("Interaction Settings")]
    public bool canInteract = true;
    public float interactionCooldown = 0.5f;
    private float lastInteractTime;

    [Header("Events")]
    public UnityEvent OnInteracted;
    public UnityEvent OnPlayerEnterRange;
    public UnityEvent OnPlayerExitRange;

    private Transform playerTransform;
    private bool isPlayerInRange = false;

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = gameObject.name;
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;

        if (isPlayerInRange && !wasInRange)
        {
            OnPlayerEnterRange?.Invoke();
            Debug.Log($"[交互提示] 靠近 {displayName} - 可交互!");
        }
        else if (!isPlayerInRange && wasInRange)
        {
            OnPlayerExitRange?.Invoke();
            Debug.Log($"[交互提示] 离开 {displayName}");
        }
    }

    public virtual void OnInteract()
    {
        if (!canInteract) return;

        if (!isPlayerInRange)
        {
            Debug.Log($"[交互] {displayName} 距离太远，无法交互");
            return;
        }

        if (Time.time - lastInteractTime < interactionCooldown) return;

        lastInteractTime = Time.time;
        OnInteracted?.Invoke();

        Debug.Log($"[交互] {displayName} ({interactType}) 被触发");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

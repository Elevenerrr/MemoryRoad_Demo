using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 墓碑交互组 - 管理一组墓碑（tombstone）的交互行为
/// 挂载到墓碑组的父对象上（Environment/FacingCamera/tombstone）
///
/// 功能：
/// 1. 所有子墓碑默认显示闪烁描边效果
/// 2. 玩家靠近后按 F 键交互
/// 3. 交互后停止描边效果，播放对话，标记任务完成
/// </summary>
public class TombstoneGroup : MonoBehaviour
{
    [Header("任务ID（用于 SceneUnlockManager 解锁条件）")]
    public string taskId = "tombstone_interacted";

    [Header("交互设置")]
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 3f;
    public float glowStopDuration = 0.5f; // 描边消失过渡时间

    [Header("交互后对话")]
    public List<DialogueLine> interactionDialogue;

    [Header("UI 提示")]
    public GameObject promptUI;

    [Header("描边效果设置")]
    public Color glowColor = new Color(0.8f, 0.8f, 1.0f, 0.8f);
    public float pulseSpeed = 2.0f;
    public float glowIntensity = 1.5f;

    [Header("引用")]
    public DialogueSystem dialogueSystem;

    // 状态
    private bool isPlayerInRange = false;
    private bool isInteracted = false;
    private Transform playerTransform;
    private SpriteRenderer[] childRenderers;
    private MaterialPropertyBlock[] propertyBlocks;
    private Color[] originalColors;
    private Material[] originalMaterials;
    private bool isGlowing = true;
    private float glowTime = 0f;

    void Start()
    {
        // 收集所有子对象的 SpriteRenderer
        CollectChildRenderers();

        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // 自动查找引用
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<DialogueSystem>();

        // 隐藏提示 UI
        if (promptUI != null)
            promptUI.SetActive(false);

        Log($"[TombstoneGroup] 初始化完成, 共 {childRenderers.Length} 个子渲染器, 使用距离检测");
    }

    void CollectChildRenderers()
    {
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[childRenderers.Length];
        originalMaterials = new Material[childRenderers.Length];
        propertyBlocks = new MaterialPropertyBlock[childRenderers.Length];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null)
            {
                originalColors[i] = childRenderers[i].color;
                originalMaterials[i] = childRenderers[i].material;
                propertyBlocks[i] = new MaterialPropertyBlock();
            }
        }
    }

    void Update()
    {
        // 描边闪烁动画
        if (isGlowing && !isInteracted)
        {
            UpdateGlowEffect();
        }

        if (isInteracted) return;

        // 距离检测玩家是否在范围内
        CheckPlayerRange();

        // 检测 F 键交互
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    /// <summary>
    /// 基于距离检测玩家是否进入/离开交互范围（避免2D/3D物理冲突）
    /// </summary>
    void CheckPlayerRange()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, transform.position);
        bool inRange = dist <= interactionRange;

        if (inRange && !isPlayerInRange)
        {
            isPlayerInRange = true;
            ShowPrompt();
            Log("[TombstoneGroup] 玩家进入范围");
        }
        else if (!inRange && isPlayerInRange)
        {
            isPlayerInRange = false;
            HidePrompt();
            Log("[TombstoneGroup] 玩家离开范围");
        }
    }

    void UpdateGlowEffect()
    {
        glowTime += Time.deltaTime;
        float intensity = Mathf.PingPong(glowTime * pulseSpeed, 1f) * (glowIntensity - 1f) + 1f;
        Color currentColor = glowColor * intensity;
        currentColor.a = Mathf.Clamp(intensity * 0.5f, 0.3f, 0.8f);

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null && propertyBlocks[i] != null)
            {
                propertyBlocks[i].SetColor("_Color", currentColor);
                childRenderers[i].SetPropertyBlock(propertyBlocks[i]);
            }
        }
    }

    void StopGlowEffect()
    {
        isGlowing = false;

        // 渐变恢复原始颜色
        StartCoroutine(FadeOutGlow());
    }

    IEnumerator FadeOutGlow()
    {
        float elapsed = 0f;
        while (elapsed < glowStopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glowStopDuration;

            for (int i = 0; i < childRenderers.Length; i++)
            {
                if (childRenderers[i] != null && propertyBlocks[i] != null)
                {
                    Color c = Color.Lerp(glowColor * glowIntensity, originalColors[i], t);
                    c.a = Mathf.Lerp(0.8f, originalColors[i].a, t);
                    propertyBlocks[i].SetColor("_Color", c);
                    childRenderers[i].SetPropertyBlock(propertyBlocks[i]);
                }
            }
            yield return null;
        }

        // 完全恢复原始状态
        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null)
            {
                childRenderers[i].SetPropertyBlock(null); // 清除 PropertyBlock
            }
        }
    }

    void TryInteract()
    {
        // 检查对话系统是否正在使用
        if (dialogueSystem != null && dialogueSystem.isDialogueActive) return;

        // 检查距离
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, transform.position);
            if (dist > interactionRange)
                return;
        }

        // 执行交互
        isInteracted = true;
        StopGlowEffect();
        HidePrompt();

        Log($"[TombstoneGroup] 墓碑已交互! 任务ID: {taskId}");

        // 标记任务完成 + 解锁手机
        if (SceneUnlockManager.Instance != null)
            SceneUnlockManager.Instance.CompleteTask(taskId);
        PhoneManager.isPhoneUnlocked = true;

        // 播放对话
        ShowInteractionDialogue();
    }

    void ShowInteractionDialogue()
    {
        if (dialogueSystem != null && interactionDialogue != null && interactionDialogue.Count > 0)
        {
            dialogueSystem.ShowDialogue(interactionDialogue);
        }
        else
        {
            // 默认对话
            if (dialogueSystem != null)
            {
                var defaultLines = new List<DialogueLine>
                {
                    new DialogueLine { speakerName = "", dialogueText = "这些墓碑......似乎在诉说着什么。", displayDuration = 3f }
                };
                dialogueSystem.ShowDialogue(defaultLines);
            }
        }
    }

    void ShowPrompt()
    {
        if (promptUI != null && !isInteracted)
            promptUI.SetActive(true);
    }

    void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    /// <summary>
    /// 外部调用：重置交互状态（用于测试）
    /// </summary>
    public void ResetInteraction()
    {
        isInteracted = false;
        isGlowing = true;
        glowTime = 0f;
        isPlayerInRange = false;
        CollectChildRenderers(); // 重新收集渲染器以恢复材质
    }

    void Log(string msg)
    {
        Debug.Log(msg);
    }
}

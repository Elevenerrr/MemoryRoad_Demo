using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PortalWithPrompt : MonoBehaviour
{
    [Header("目标场景")]
    public int targetSceneIndex = 0;

    [Header("UI 提示")]
    public GameObject FintoUI;
    public KeyCode interactKey = KeyCode.F;

    [Header("解锁条件（SceneUnlockManager 中注册的任务ID，留空则无条件）")]
    public string[] requiredConditions = new string[0];

    [Header("未满足条件时播放的对话")]
    public List<DialogueLine> blockedDialogue;

    [Header("Jump 引用")]
    public Jump2 jumpScript;

    private bool isPlayerInRange = false;
    private bool hasTriggered = false;
    private DialogueSystem dialogueSystem;

    private void Start()
    {
        FintoUI.SetActive(false);

        // 确保 Collider 是触发器
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
        else
            Debug.LogError("PortalWithPrompt 需要 Collider 组件！");

        if (jumpScript == null)
        {
            jumpScript = FindObjectOfType<Jump2>();
            if (jumpScript == null)
                Debug.LogWarning("未找到 Jump 脚本，将无法跳转！");
        }

        // 自动查找对话系统
        dialogueSystem = FindObjectOfType<DialogueSystem>();

        if (FintoUI != null)
            FintoUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange || hasTriggered)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            // 检查解锁条件
            if (requiredConditions != null && requiredConditions.Length > 0
                && SceneUnlockManager.Instance != null
                && !SceneUnlockManager.Instance.AreAllConditionsMet(requiredConditions))
            {
                // 条件不满足，播放阻止对话
                ShowBlockedDialogue();
                return;
            }

            // 条件满足（或无条件），执行跳转
            if (jumpScript != null)
            {
                jumpScript.JumpToTargetScene(targetSceneIndex);
                hasTriggered = true;
                Debug.Log($"[传送门] 按 {interactKey} 触发跳转到场景 {targetSceneIndex}");
            }
            else
            {
                Debug.LogError("Jump 脚本未找到，无法跳转！");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInRange = true;
        hasTriggered = false;           // 重置触发标志，以便下次进入可再次触发

        if (FintoUI != null)
            FintoUI.SetActive(true);

        Debug.Log("[传送门] 玩家进入，显示提示");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInRange = false;
        hasTriggered = false;           // 重置，以便再次进入

        if (FintoUI != null)
            FintoUI.SetActive(false);

        Debug.Log("[传送门] 玩家离开，隐藏提示");
    }

    void ShowBlockedDialogue()
    {
        if (dialogueSystem != null && blockedDialogue != null && blockedDialogue.Count > 0)
        {
            dialogueSystem.ShowDialogue(blockedDialogue);
        }
        else if (dialogueSystem != null)
        {
            // 默认阻止对话
            var defaultLines = new List<DialogueLine>
            {
                new DialogueLine { speakerName = "", dialogueText = "好像漏了些什么......", displayDuration = 2.5f }
            };
            dialogueSystem.ShowDialogue(defaultLines);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PortalWithPrompt : MonoBehaviour
{
    [Header("目标场景")]
    public int targetSceneIndex = 0;            // 要跳转的场景索引（Build Settings 中）

    [Header("UI 提示")]
    public GameObject FintoUI;                 // 提示 UI 对象（比如一个 Panel 或 Text）
    public KeyCode interactKey = KeyCode.F;     // 交互按键，默认 F

    [Header("Jump 引用（可选，拖拽更可靠）")]
    public Jump2 jumpScript;

    private bool isPlayerInRange = false;       // 玩家是否在范围内
    private bool hasTriggered = false;          // 防止重复触发（跳转过程中）

    private void Start()
    {
        FintoUI.SetActive(false);

        // 确保 Collider 是触发器
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
        else
            Debug.LogError("PortalWithPrompt 需要 Collider 组件！");

        // 如果未手动指定 Jump，自动查找
        if (jumpScript == null)
        {
            jumpScript = FindObjectOfType<Jump2>();
            if (jumpScript == null)
                Debug.LogWarning("未找到 Jump 脚本，将无法跳转！");
        }

        // 初始隐藏提示
        if (FintoUI != null)
            FintoUI.SetActive(false);
    }

    private void Update()
    {
        // 只有玩家在范围内且未触发跳转时，才检测按键
        if (!isPlayerInRange || hasTriggered)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            // 执行跳转
            if (jumpScript != null)
            {
                // 调用 Jump2 的通用方法（需要你在 Jump2 中添加 JumpToScene(int)）
                jumpScript.JumpToTargetScene(targetSceneIndex);
                hasTriggered = true;    // 防止多次触发（跳转过程中场景可能还未切换）
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
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 场景解锁条件管理器（单例）
/// 用于管理每个场景中传送门的解锁前提条件。
/// 每个场景的不同 TransDoor 可以有不同的解锁条件。
///
/// 使用方式：
/// 1. 在每个场景中确保存在此组件（建议挂载在场景根节点或 DontDestroyOnLoad 对象上）
/// 2. 交互物体完成任务时调用 CompleteTask(taskId)
/// 3. TransDoor/传送门检查 IsAllConditionsMet(conditions) 来决定是否放行
/// </summary>
public class SceneUnlockManager : MonoBehaviour
{
    public static SceneUnlockManager Instance { get; private set; }

    [Header("当前场景已完成的任务")]
    private HashSet<string> completedTasks = new HashSet<string>();

    [Header("调试")]
    public bool enableDebugLog = true;

    // 任务完成事件
    public UnityEvent<string> OnTaskCompleted;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 场景加载时清空任务（每个场景独立）
        completedTasks.Clear();
        Log("[SceneUnlockManager] 初始化，任务列表已清空");
    }

    /// <summary>
    /// 标记一个任务为已完成
    /// </summary>
    public void CompleteTask(string taskId)
    {
        if (string.IsNullOrEmpty(taskId)) return;

        if (completedTasks.Contains(taskId))
        {
            Log($"[SceneUnlockManager] 任务已完成（重复）: {taskId}");
            return;
        }

        completedTasks.Add(taskId);
        Log($"[SceneUnlockManager] 任务完成: {taskId} (当前共 {completedTasks.Count} 个)");
        OnTaskCompleted?.Invoke(taskId);
    }

    /// <summary>
    /// 检查某个任务是否已完成
    /// </summary>
    public bool IsTaskCompleted(string taskId)
    {
        return completedTasks.Contains(taskId);
    }

    /// <summary>
    /// 检查是否所有条件都已满足
    /// </summary>
    public bool AreAllConditionsMet(string[] requiredTaskIds)
    {
        if (requiredTaskIds == null || requiredTaskIds.Length == 0) return true;

        foreach (string taskId in requiredTaskIds)
        {
            if (!completedTasks.Contains(taskId))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取第一个未满足的条件ID（用于提示信息）
    /// </summary>
    public string GetFirstUnmetCondition(string[] requiredTaskIds)
    {
        if (requiredTaskIds == null || requiredTaskIds.Length == 0) return null;

        foreach (string taskId in requiredTaskIds)
        {
            if (!completedTasks.Contains(taskId))
                return taskId;
        }
        return null;
    }

    /// <summary>
    /// 获取当前所有已完成的任务列表（只读）
    /// </summary>
    public HashSet<string> GetCompletedTasks()
    {
        return new HashSet<string>(completedTasks);
    }

    /// <summary>
    /// 重置所有任务（谨慎使用）
    /// </summary>
    public void ResetAllTasks()
    {
        completedTasks.Clear();
        Log("[SceneUnlockManager] 所有任务已重置");
    }

    private void Log(string message)
    {
        if (enableDebugLog)
            Debug.Log(message);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

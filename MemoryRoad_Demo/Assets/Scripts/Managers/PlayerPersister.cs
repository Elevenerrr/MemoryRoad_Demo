using UnityEngine;

/// <summary>
/// 玩家持久化 - 确保 GamePlayer 在场景切换时不会被销毁
/// 挂载到 GamePlayer 预制体上。
///
/// 原理：
/// 1. 第一个场景加载 GamePlayer 时，注册为 DontDestroyOnLoad
/// 2. 后续场景如果也有 GamePlayer，检测到已存在则自动销毁重复的
/// 3. 保证整个游戏中始终只有一个 GamePlayer 实例
/// </summary>
public class PlayerPersister : MonoBehaviour
{
    private static GameObject persistentPlayer = null;
    private static string playerID = "GamePlayer_Persistent";

    void Awake()
    {
        // 如果已经有一个持久化的玩家，销毁自己（防止重复）
        if (persistentPlayer != null && persistentPlayer != gameObject)
        {
            Debug.Log("[PlayerPersister] 检测到重复 GamePlayer，销毁当前实例");
            Destroy(gameObject);
            return;
        }

        // 第一个实例：标记为持久化
        if (persistentPlayer == null)
        {
            persistentPlayer = gameObject;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PlayerPersister] GamePlayer 已注册为跨场景持久化");
        }
    }

    void OnDestroy()
    {
        // 如果被销毁的是持久化实例，清除引用
        if (persistentPlayer == gameObject)
        {
            persistentPlayer = null;
            Debug.Log("[PlayerPersister] 持久化 GamePlayer 已销毁");
        }
    }

    /// <summary>
    /// 外部调用：获取当前的持久化玩家（可能为 null）
    /// </summary>
    public static GameObject GetPersistentPlayer()
    {
        return persistentPlayer;
    }
}

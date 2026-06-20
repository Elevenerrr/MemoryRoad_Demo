using UnityEngine;

/// <summary>
/// 场景持久化 - 标记该 GameObject 在场景切换时不被销毁
/// 用于 Main Camera 等需要在所有场景中保持存在的对象。
/// 挂载到需要跨场景保留的对象上即可。
/// </summary>
public class ScenePersister : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

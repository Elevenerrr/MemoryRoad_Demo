using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 场景持久化 - 标记该 GameObject 在场景切换时不被销毁
/// 同名对象自动去重：第一个注册的保留，后续重复的自动销毁
/// </summary>
public class ScenePersister : MonoBehaviour
{
    private static Dictionary<string, GameObject> persistentObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        string id = gameObject.name;

        if (persistentObjects.ContainsKey(id))
        {
            if (persistentObjects[id] != gameObject)
            {
                Debug.Log($"[ScenePersister] 检测到重复对象 [{id}]，销毁当前实例");
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            persistentObjects.Add(id, gameObject);
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[ScenePersister] 对象 [{id}] 已注册为跨场景持久化");
        }
    }

    void OnDestroy()
    {
        if (persistentObjects.ContainsKey(gameObject.name) && persistentObjects[gameObject.name] == gameObject)
        {
            persistentObjects.Remove(gameObject.name);
        }
    }
}

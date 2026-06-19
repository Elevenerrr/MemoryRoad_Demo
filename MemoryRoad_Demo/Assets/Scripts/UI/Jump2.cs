using UnityEngine;

public class Jump2 : MonoBehaviour
{
    public void JumpToTargetScene(int sceneIndex)
    {
        // 查找场景中第一个挂载了 Jump 的组件（不保证是主实例）
        Jump jump = FindObjectOfType<Jump>();
        if (jump != null)
            jump.JumpToScene(sceneIndex);
        else
            Debug.LogError("场景中未找到 Jump 组件！");
    }
}
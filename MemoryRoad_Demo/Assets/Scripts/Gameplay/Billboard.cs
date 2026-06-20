using UnityEngine;

/// <summary>
/// 让2D精灵（纸片人）始终面向摄像机，用于2D+3D混合风格游戏。
/// 挂载到需要面向摄像机的2D对象上（树、角色、蘑菇等）。
/// </summary>
[AddComponentMenu("MemoryRoad/Billboard")]
public class Billboard : MonoBehaviour
{
    [Tooltip("是否只在Y轴旋转（保持直立）")]
    public bool yAxisOnly = true;

    [Tooltip("是否每帧更新（关闭后只在Awake时朝向一次）")]
    public bool updateEveryFrame = true;

    [Tooltip("目标摄像机（留空则自动获取Main Camera）")]
    public Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        FaceCamera();
    }

    private void LateUpdate()
    {
        if (!updateEveryFrame || targetCamera == null)
            return;
        FaceCamera();
    }

    void FaceCamera()
    {
        if (targetCamera == null) return;

        Vector3 direction = targetCamera.transform.position - transform.position;

        if (yAxisOnly)
        {
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}

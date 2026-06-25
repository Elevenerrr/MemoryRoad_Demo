using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // 需要 Dictionary

public class Jump : MonoBehaviour
{
    // 静态字典：存储所有已注册的跨场景物体（ID -> GameObject）
    private static Dictionary<string, GameObject> persistentObjects = new Dictionary<string, GameObject>();

    [Header("唯一标识（建议每个物体设置不同名称）")]
    public string uniqueID; // 如果不填，默认使用 gameObject.name

    [Header("动画时长")]
    [SerializeField] private float animationDuration = 1f;
    private Animator fadeAnimator;

    void Awake()
    {
        // 如果 uniqueID 为空，则使用物体名称
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = gameObject.name;

        // 检查字典中是否已存在相同ID的物体
        if (persistentObjects.ContainsKey(uniqueID))
        {
            // 如果已存在，且不是自己（因为可能同一个物体被多次触发），则销毁当前物体
            if (persistentObjects[uniqueID] != gameObject)
            {
                Debug.Log($"检测到重复的跨场景物体 [{uniqueID}]，销毁新生成的实例。");
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // 首次出现：注册并标记为跨场景保留
            persistentObjects.Add(uniqueID, gameObject);
            DontDestroyOnLoad(gameObject);
            Debug.Log($"物体 [{uniqueID}] 已注册为跨场景保留。");
        }
    }


    // ---------- 跳转方法（转发给主实例） ----------
    public void JumpToScene(int sceneIndex)
    {
        // 找到当前物体的主实例（即字典中存储的那个）
        GameObject mainObj = persistentObjects.ContainsKey(uniqueID) ? persistentObjects[uniqueID] : null;
        if (mainObj == null || mainObj == gameObject)
        {
            // 当前就是主实例，直接执行
            if (fadeAnimator == null)
            {
                Debug.LogError("fadeAnimator 未初始化");
                return;
            }
            StartCoroutine(TransitionAndLoad(sceneIndex));
        }
        else
        {
            // 转发给主实例
            Jump mainJump = mainObj.GetComponent<Jump>();
            if (mainJump != null)
                mainJump.JumpToScene(sceneIndex);
            else
                Debug.LogError("主实例丢失 Jump 组件");
        }
    }



    public void Jump0() { JumpToScene(0); }
    public void Jump1() { JumpToScene(1); }
    public void Jump2() { JumpToScene(2); }
    public void Jump3() { JumpToScene(3); }
    public void Jump4() { JumpToScene(4); }


    void Start()
    {
        // 动态查找 FadeImage（必须在 Start 中查找，确保激活）
        GameObject fadeObj = GameObject.Find("FadeImage"); // 使用你的FadeImage的GameObject名称
        if (fadeObj != null)
        {
            Image img = fadeObj.GetComponent<Image>();
            if (img != null)
                fadeAnimator = img.GetComponent<Animator>();
            else
                print("FadeImage 缺少 Image 组件！");
        }
        else
        {
            print("场景中找不到名为 FadeImage 的物体！请检查名称。");
        }
    }


    /// <summary>
    /// 场景传送期间是否冻结玩家移动（防止漂移）
    /// </summary>
    public static bool IsTransitioning { get; private set; } = false;

    // 过渡动画和加载场景的协程
    private IEnumerator TransitionAndLoad(int sceneIndex)
    {
        if (fadeAnimator == null)
        {
            print("fadeAnimator 未初始化，无法播放动画！");
        }

        // 标记传送开始，冻结移动
        IsTransitioning = true;

        // 1. 播放 fadein（1秒）
        if (fadeAnimator != null) { fadeAnimator.Play("fadein", 0, 0f); }
        yield return new WaitForSeconds(animationDuration);

        // 2. 异步加载场景，但不自动激活
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        // 等待场景加载完成（progress == 0.9 表示加载完毕）
        while (asyncLoad.progress < 0.9f)
            yield return null;
        // 3. 激活新场景
        asyncLoad.allowSceneActivation = true;

        // 等待场景真正激活（allowSceneActivation 后需要再等一帧）
        yield return null;

        // 将玩家传送到新场景的出生点
        TeleportPlayerToSpawnPoint();

        // 等待一帧让物理引擎同步
        yield return null;

        // 解冻移动
        IsTransitioning = false;

        // 4. 场景已加载完毕，播放 fadeout（1秒）
        if (fadeAnimator != null) { fadeAnimator.Play("fadeout", 0, 0f); }
        yield return new WaitForSeconds(animationDuration);

    }

    /// <summary>
    /// 将玩家传送到当前场景的 PlayerSpawnPoint 位置
    /// 每个场景中放置一个名为 "PlayerSpawnPoint" 的空物体作为出生点即可
    /// </summary>
    private void TeleportPlayerToSpawnPoint()
    {
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint == null) return; // 没有出生点则不处理

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[Jump] 找到了 PlayerSpawnPoint 但未找到玩家！");
            return;
        }

        Vector3 targetPos = spawnPoint.transform.position;

        // 使用 Rigidbody 传送（比直接设 transform.position 更可靠）
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPos;
            // 强制物理引擎立即同步，避免一帧延迟
            Physics.SyncTransforms();
        }
        else
        {
            player.transform.position = targetPos;
        }

        // 瞬移相机到正确位置（避免阻尼平滑导致的视觉偏移）
        SnapCameraToPlayer();

        Debug.Log($"[Jump] 玩家已传送到出生点: {targetPos}");
    }

    /// <summary>
    /// 瞬移相机到玩家正后方（重置 CameraFollow 的偏移）
    /// </summary>
    private void SnapCameraToPlayer()
    {
        CameraFollow camFollow = FindObjectOfType<CameraFollow>();
        if (camFollow != null && camFollow.target != null)
        {
            // 直接设置相机位置到目标后方，绕过 Lerp 阻尼
            Vector3 offset = new Vector3(0, 5, -10);
            camFollow.transform.position = camFollow.target.position + offset;
            camFollow.transform.LookAt(camFollow.target);
            Debug.Log("[Jump] 相机已瞬移到玩家后方");
        }
    }
}
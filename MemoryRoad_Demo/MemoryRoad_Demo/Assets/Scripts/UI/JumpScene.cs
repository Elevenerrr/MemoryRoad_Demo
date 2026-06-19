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

    
    // 过渡动画和加载场景的协程
    private IEnumerator TransitionAndLoad(int sceneIndex)
    {
        if (fadeAnimator == null)
        {
            print("fadeAnimator 未初始化，无法播放动画！");
        }

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
        

        // 4. 场景已加载完毕，播放 fadeout（1秒）
        if (fadeAnimator != null) { fadeAnimator.Play("fadeout", 0, 0f); }
        yield return new WaitForSeconds(animationDuration);

    }


}
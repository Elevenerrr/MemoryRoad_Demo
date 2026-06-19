using UnityEngine;
using System.Collections;

public class TipHider : MonoBehaviour
{
    [Header("Tip Settings")]
    public string tipAction = "Audio";
    public KeyCode triggerKey;
    public float fadeDuration = 1.0f;

    private bool actionPerformed = false;
    private SpriteRenderer spriteRenderer;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        Debug.Log($"TipHider.Start() called on {gameObject.name}");
        Debug.Log($"Tip action: {tipAction}, Trigger key: {triggerKey}");
        
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 注册事件监听器
        if (PhoneManager.Instance != null)
        {
            if (tipAction == "Audio")
            {
                PhoneManager.Instance.OnAudioPlayed.AddListener(OnActionPerformed);
                Debug.Log("Added OnAudioPlayed listener");
            }
            else if (tipAction == "Overlay")
            {
                // 覆盖功能没有直接的事件，需要在 Update 中监听按键
                Debug.Log("Will listen for overlay key in Update");
            }
            else if (tipAction == "Photo")
            {
                PhoneManager.Instance.OnPhotoTaken.AddListener(OnActionPerformed);
                Debug.Log("Added OnPhotoTaken listener");
            }
        }
        else
        {
            Debug.LogError("PhoneManager.Instance is null");
        }
    }

    private void Update()
    {
        // 监听触发键
        if (Input.GetKeyDown(triggerKey) && !actionPerformed)
        {
            OnActionPerformed();
        }
    }

    private void OnActionPerformed()
    {
        if (!actionPerformed)
        {
            actionPerformed = true;
            StartCoroutine(FadeOut());
            Debug.Log($"Action performed for tip: {gameObject.name}, starting fade out");
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = 1f;
        
        // 获取初始透明度
        if (spriteRenderer != null)
        {
            startAlpha = spriteRenderer.color.a;
        }
        else if (canvasGroup != null)
        {
            startAlpha = canvasGroup.alpha;
        }
        
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            
            // 应用透明度
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 完全隐藏对象
        gameObject.SetActive(false);
        Debug.Log($"Tip {gameObject.name} has faded out and been hidden");
    }

    private void OnDestroy()
    {
        // 移除事件监听器
        if (PhoneManager.Instance != null)
        {
            if (tipAction == "Audio")
            {
                PhoneManager.Instance.OnAudioPlayed.RemoveListener(OnActionPerformed);
            }
            else if (tipAction == "Photo")
            {
                PhoneManager.Instance.OnPhotoTaken.RemoveListener(OnActionPerformed);
            }
            Debug.Log("Removed event listeners from PhoneManager");
        }
    }
}
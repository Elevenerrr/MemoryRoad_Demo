using System.Collections;
using UnityEngine;

public class OutlineGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public float glowIntensity = 1.5f;
    public float pulseSpeed = 2.0f;

    [Header("References")]
    public MeshRenderer meshRenderer;
    public InteractableObject interactableObject;

    private Material originalMaterial;
    private bool isGlowing = true;

    private void Start()
    {
        Debug.Log("OutlineGlowEffect.Start() called on " + gameObject.name);

        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            Debug.Log("MeshRenderer found: " + (meshRenderer != null));
        }

        if (interactableObject == null)
        {
            interactableObject = GetComponentInParent<InteractableObject>();
            Debug.Log("InteractableObject found: " + (interactableObject != null));
        }

        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
            Debug.Log("Original material: " + originalMaterial.name);

            // 开始发光动画
            StartCoroutine(GlowAnimation());
            Debug.Log("Started glow animation");
        }
        else
        {
            Debug.LogError("No MeshRenderer found");
        }

        if (interactableObject != null)
        {
            interactableObject.OnInteracted.AddListener(OnInteract);
            Debug.Log("Added OnInteracted listener");
        }
        else
        {
            Debug.LogError("No InteractableObject found");
        }
    }

    private IEnumerator GlowAnimation()
    {
        float time = 0f;
        while (isGlowing)
        {
            time += Time.deltaTime;
            float intensity = Mathf.PingPong(time * pulseSpeed, 1f) * (glowIntensity - 1f) + 1f;
            // 使用固定的发光颜色
            Color currentColor = new Color(0.8f, 0.8f, 1.0f, 0.8f) * intensity;
            currentColor.a = Mathf.Clamp(currentColor.a, 0.3f, 0.8f);

            if (meshRenderer != null && originalMaterial != null)
            {
                // 创建一个新的材质实例，保持原始材质的所有属性
                Material tempMaterial = new Material(originalMaterial);
                // 修改颜色属性，保持透明度
                tempMaterial.color = currentColor;
                meshRenderer.material = tempMaterial;

                // 每2秒记录一次发光颜色
                if (Mathf.FloorToInt(time * 0.5f) % 4 == 0)
                {
                    Debug.Log("Glow color: " + currentColor.ToString());
                }
            }
            yield return null;
        }
    }

    private void OnInteract()
    {
        Debug.Log("OnInteract called");
        if (isGlowing)
        {
            StopAllCoroutines();
            isGlowing = false;
            if (meshRenderer != null && originalMaterial != null)
            {
                // 恢复原始材质
                meshRenderer.material = originalMaterial;
                Debug.Log("Stopped glow, restored original material");
            }
        }
    }

    private void OnDestroy()
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }

        if (interactableObject != null)
        {
            interactableObject.OnInteracted.RemoveListener(OnInteract);
        }
    }
}
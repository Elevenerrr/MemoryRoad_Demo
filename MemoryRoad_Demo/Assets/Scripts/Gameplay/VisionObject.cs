using System.Collections;
using UnityEngine;

public class VisionObject : InteractableObject
{
    [Header("Vision Settings")]
    public float effectDuration = 3f;
    public Collider wallCollider;
    public Color visionOverlayColor = new Color(1f, 1f, 1f, 0.9f);

    private bool isEffectActive = false;
    private Camera mainCamera;
    private Color originalCameraBgColor;

    protected override void Start()
    {
        base.Start();
        interactType = InteractableType.Vision;
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraBgColor = mainCamera.backgroundColor;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();
        StartCoroutine(VisionEffectCoroutine());
    }

    IEnumerator VisionEffectCoroutine()
    {
        isEffectActive = true;

        if (wallCollider != null)
        {
            wallCollider.enabled = false;
        }

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = visionOverlayColor;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        Debug.Log("[视觉] 视野变白，穿墙激活");

        yield return new WaitForSeconds(effectDuration);

        if (wallCollider != null)
        {
            wallCollider.enabled = true;
        }

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = originalCameraBgColor;
            mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        isEffectActive = false;
        Debug.Log("[视觉] 效果结束");
    }
}

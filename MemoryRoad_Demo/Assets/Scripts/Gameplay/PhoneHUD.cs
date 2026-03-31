using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhoneHUD : MonoBehaviour
{
    [Header("Phone UI")]
    public GameObject phoneUIRoot;
    public Image functionIcon1;
    public Image functionIcon2;
    public Image functionIcon3;
    public Image functionIcon4;
    public TextMeshProUGUI statusText;

    [Header("Camera Frame UI")]
    public GameObject cameraFrame;
    public Image frameBorder;
    public TextMeshProUGUI frameHintText;

    [Header("Visual Feedback")]
    public Image screenFlash;
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    [Header("Indicator Icons")]
    public Sprite iconPhoto;
    public Sprite iconVision;
    public Sprite iconAudio;
    public Sprite iconOverlay;
    public Sprite iconUnknown;

    [Header("Frame Colors")]
    public Color photoFrameColor = new Color(0f, 1f, 0f, 0.8f);
    public Color visionFrameColor = new Color(0f, 0.5f, 1f, 0.8f);
    public Color audioFrameColor = new Color(1f, 0.5f, 0f, 0.8f);
    public Color overlayFrameColor = new Color(1f, 0f, 1f, 0.8f);
    public Color noTargetColor = new Color(1f, 1f, 1f, 0.3f);

    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private InteractableObject lastTarget;

    void Start()
    {
        canvasGroup = phoneUIRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = phoneUIRoot.AddComponent<CanvasGroup>();
        }

        HidePhoneUI();
        HideCameraFrame();

        if (PhoneManager.Instance != null)
        {
            PhoneManager.Instance.OnPhoneEquipped.AddListener(OnPhoneEquipped);
            PhoneManager.Instance.OnPhoneUnequipped.AddListener(OnPhoneUnequipped);
            PhoneManager.Instance.OnPhoneActivated.AddListener(OnPhoneActivated);
            PhoneManager.Instance.OnPhoneDeactivated.AddListener(OnPhoneDeactivated);
            PhoneManager.Instance.OnPhotoTaken.AddListener(OnPhotoTaken);
        }
    }

    void OnPhoneEquipped()
    {
        Debug.Log("[HUD] 手机装备");
        ShowPhoneUI();
    }

    void OnPhoneUnequipped()
    {
        Debug.Log("[HUD] 手机卸下");
        HidePhoneUI();
        HideCameraFrame();
    }

    void OnPhoneActivated()
    {
        SetFunctionIcons(true);
        UpdateStatus("手机功能已开启");
    }

    void OnPhoneDeactivated()
    {
        SetFunctionIcons(false);
        UpdateStatus("手机息屏");
        HideCameraFrame();
    }

    void OnPhotoTaken()
    {
        StartCoroutine(FlashScreen());
    }

    void ShowPhoneUI()
    {
        isVisible = true;
        phoneUIRoot.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    void HidePhoneUI()
    {
        isVisible = false;
        phoneUIRoot.SetActive(false);
    }

    void ShowCameraFrame(InteractableObject target)
    {
        if (cameraFrame == null) return;

        cameraFrame.SetActive(true);

        Color frameColor = noTargetColor;
        string hintText = "未对准可交互物体";
        Sprite icon = iconUnknown;

        if (target != null)
        {
            switch (target.interactType)
            {
                case InteractableType.Photo:
                    frameColor = photoFrameColor;
                    hintText = "📷 按1 拍照";
                    icon = iconPhoto;
                    break;
                case InteractableType.Vision:
                    frameColor = visionFrameColor;
                    hintText = "👁️ 按1 穿墙";
                    icon = iconVision;
                    break;
                case InteractableType.Audio:
                    frameColor = audioFrameColor;
                    hintText = "🔊 按3 播放录音";
                    icon = iconAudio;
                    break;
                case InteractableType.Overlay:
                    frameColor = overlayFrameColor;
                    hintText = "🖼️ 按4 选择照片";
                    icon = iconOverlay;
                    break;
            }
        }

        if (frameBorder != null)
        {
            frameBorder.color = frameColor;
        }

        if (frameHintText != null)
        {
            frameHintText.text = hintText;
        }
    }

    void HideCameraFrame()
    {
        if (cameraFrame != null)
        {
            cameraFrame.SetActive(false);
        }
    }

    void SetFunctionIcons(bool active)
    {
        Color iconColor = active ? Color.white : Color.gray;

        if (functionIcon1 != null)
        {
            functionIcon1.sprite = iconPhoto;
            functionIcon1.color = iconColor;
        }
        if (functionIcon2 != null)
        {
            functionIcon2.sprite = iconVision;
            functionIcon2.color = iconColor;
        }
        if (functionIcon3 != null)
        {
            functionIcon3.sprite = iconAudio;
            functionIcon3.color = iconColor;
        }
        if (functionIcon4 != null)
        {
            functionIcon4.sprite = iconOverlay;
            functionIcon4.color = iconColor;
        }
    }

    void UpdateStatus(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }

    IEnumerator FlashScreen()
    {
        if (screenFlash == null) yield break;

        screenFlash.gameObject.SetActive(true);
        screenFlash.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            screenFlash.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        screenFlash.gameObject.SetActive(false);
    }

    void Update()
    {
        if (PhoneManager.Instance == null) return;

        if (PhoneManager.Instance.isPhoneActive)
        {
            int photoCount = PhoneManager.Instance.GetPhotoCount();
            UpdateStatus($"已拍摄: {photoCount} 张");

            InteractableObject currentTarget = PhoneManager.Instance.currentTarget;

            if (currentTarget != lastTarget)
            {
                lastTarget = currentTarget;
                if (currentTarget != null)
                {
                    ShowCameraFrame(currentTarget);
                }
                else
                {
                    ShowCameraFrame(null);
                }
            }
        }
    }
}

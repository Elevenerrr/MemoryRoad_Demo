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
    public Image functionIcon3;
    public Image functionIcon4;

    [Header("Visual Feedback")]
    public Image screenFlash;
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    [Header("Indicator Icons")]
    public Sprite iconPhoto;
    public Sprite iconVision;
    public Sprite iconAudio;
    public Sprite iconOverlay;

    private CanvasGroup canvasGroup;
    private bool isVisible = false;

    void Start()
    {
        canvasGroup = phoneUIRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = phoneUIRoot.AddComponent<CanvasGroup>();
        }

        HidePhoneUI();

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
    }

    void OnPhoneActivated()
    {
        SetFunctionIcons(true);
    }

    void OnPhoneDeactivated()
    {
        SetFunctionIcons(false);
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

    void SetFunctionIcons(bool active)
    {
        Color iconColor = active ? Color.white : Color.gray;

        if (functionIcon1 != null)
        {
            functionIcon1.sprite = iconPhoto;
            functionIcon1.color = iconColor;
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
}

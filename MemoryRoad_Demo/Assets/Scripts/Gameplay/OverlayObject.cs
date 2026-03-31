using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayObject : InteractableObject
{
    [Header("Overlay Settings")]
    public string correctPhotoId;
    public Transform popupPosition;
    public GameObject overlayPanel;
    public List<Button> photoButtons;

    private bool isOverlayActive = false;

    protected override void Start()
    {
        base.Start();
        interactType = InteractableType.Overlay;
    }

    public override void OnInteract()
    {
        base.OnInteract();
        ShowOverlay();
    }

    void ShowOverlay()
    {
        if (overlayPanel == null)
        {
            Debug.LogWarning("[覆盖] 请在 Inspector 中设置 overlayPanel");
            return;
        }

        isOverlayActive = true;
        overlayPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[覆盖] 弹出选择界面");
    }

    public void OnPhotoSelected(int index)
    {
        if (!isOverlayActive) return;

        bool success = CheckPhoto(index);

        if (success)
        {
            Debug.Log("[覆盖] 选择正确！通关！");
            OnSuccess();
        }
        else
        {
            Debug.Log("[覆盖] 选择错误，请重试");
        }

        CloseOverlay();
    }

    bool CheckPhoto(int index)
    {
        if (PhoneManager.Instance == null) return false;

        string selectedPhotoId = PhoneManager.Instance.takenPhotos.Count > index
            ? PhoneManager.Instance.takenPhotos[index]
            : "";

        return selectedPhotoId == correctPhotoId;
    }

    void OnSuccess()
    {
        Debug.Log("[覆盖] 谜题解开！");
    }

    public void CloseOverlay()
    {
        isOverlayActive = false;
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

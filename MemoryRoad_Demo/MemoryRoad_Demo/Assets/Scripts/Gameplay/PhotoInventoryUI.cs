using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotoInventoryUI : MonoBehaviour
{
    [Header("Inventory Panel")]
    public GameObject inventoryPanel;
    public HorizontalLayoutGroup photoContainer;
    public ContentSizeFitter containerSizeFitter;
    public GameObject photoButtonPrefab;

    [Header("Info Display")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI hintText;

    [Header("Settings")]
    public float panelShowDelay = 0.1f;

    private bool isInventoryOpen = false;
    private OverlayObject currentOverlay;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    void Start()
    {
        HideInventory();
    }

    public void ShowInventory(OverlayObject overlay)
    {
        currentOverlay = overlay;
        isInventoryOpen = true;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "照片背包";
        }

        if (hintText != null)
        {
            hintText.text = "选择正确的照片进行覆盖";
        }

        RefreshPhotoList();
    }

    public void HideInventory()
    {
        isInventoryOpen = false;
        currentOverlay = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    void RefreshPhotoList()
    {
        if (photoContainer == null || photoButtonPrefab == null) return;

        foreach (Transform child in photoContainer.transform)
        {
            Destroy(child.gameObject);
        }

        if (PhotoManager.Instance == null)
        {
            if (hintText != null) hintText.text = "没有照片！";
            return;
        }

        List<PhotoData> photos = PhotoManager.Instance.photos;

        if (photos.Count == 0)
        {
            if (hintText != null) hintText.text = "还没有拍过照片！";
            return;
        }

        foreach (var photo in photos)
        {
            GameObject cardObj = Instantiate(photoButtonPrefab, photoContainer.transform);

            PhotoButton button = cardObj.GetComponent<PhotoButton>();
            if (button != null)
            {
                button.Setup(photo, OnPhotoSelected);
            }
        }

        if (containerSizeFitter != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerSizeFitter.GetComponent<RectTransform>());
        }

        if (hintText != null)
        {
            hintText.text = $"共 {photos.Count} 张照片";
        }
    }

    void OnPhotoSelected(PhotoData photo)
    {
        if (currentOverlay == null) return;

        Debug.Log($"[背包] 选择了照片: {photo.photoId}");

        currentOverlay.OnPhotoSelected(photo);

        HideInventory();
    }

    void Update()
    {
        if (isInventoryOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            HideInventory();
        }
    }
}

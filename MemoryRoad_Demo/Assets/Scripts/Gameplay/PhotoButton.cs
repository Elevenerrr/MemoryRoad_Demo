using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotoButton : MonoBehaviour
{
    public Image photoImage;
    public TextMeshProUGUI photoNameText;
    public Button button;

    private PhotoData photoData;
    private Action<PhotoData> onSelectCallback;

    public void Setup(PhotoData photo, Action<PhotoData> callback)
    {
        photoData = photo;
        onSelectCallback = callback;

        if (photoImage != null)
        {
            if (photo.photoSprite != null)
            {
                photoImage.sprite = photo.photoSprite;
            }
            else
            {
                photoImage.color = Color.gray;
            }
        }

        if (photoNameText != null)
        {
            photoNameText.text = photo.photoName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelectCallback?.Invoke(photoData));
        }
    }
}

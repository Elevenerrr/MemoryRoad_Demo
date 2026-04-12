using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhotoData
{
    public string photoId;
    public string photoName;
    public Sprite photoSprite;
    public bool isCorrectPhoto;

    public PhotoData(string id, string name, Sprite sprite = null, bool correct = false)
    {
        photoId = id;
        photoName = name;
        photoSprite = sprite;
        isCorrectPhoto = correct;
    }
}

public class PhotoManager : MonoBehaviour
{
    public static PhotoManager Instance { get; private set; }

    [Header("Photo Storage")]
    public List<PhotoData> photos = new List<PhotoData>();
    public int maxPhotos = 10;

    [Header("Photo Sources")]
    public List<PhotoSource> photoSources;

    void Awake()
    {
        Instance = this;
    }

    public bool AddPhoto(string photoId)
    {
        if (photos.Count >= maxPhotos)
        {
            Debug.Log("[照片] 相册已满！");
            return false;
        }

        if (HasPhoto(photoId))
        {
            Debug.Log("[照片] 已经拍过 {photoId}");
            return false;
        }

        PhotoSource source = GetPhotoSource(photoId);
        string photoName = source != null ? source.displayName : photoId;
        Sprite photoSprite = source != null ? source.photoSprite : null;

        PhotoData newPhoto = new PhotoData(photoId, photoName, photoSprite);
        photos.Add(newPhoto);

        Debug.Log($"[照片] 拍摄成功: {photoId} - {photoName}");
        return true;
    }

    public bool HasPhoto(string photoId)
    {
        return photos.Exists(p => p.photoId == photoId);
    }

    public PhotoData GetPhoto(int index)
    {
        if (index >= 0 && index < photos.Count)
            return photos[index];
        return null;
    }

    public int GetPhotoCount()
    {
        return photos.Count;
    }

    public void ClearPhotos()
    {
        photos.Clear();
        Debug.Log("[照片] 相册已清空");
    }

    PhotoSource GetPhotoSource(string photoId)
    {
        foreach (var source in photoSources)
        {
            if (source.objectId == photoId)
                return source;
        }
        return null;
    }
}

[System.Serializable]
public class PhotoSource
{
    public string objectId;
    public string displayName;
    public Sprite photoSprite;
    public bool isCorrectPhoto;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OverlayObject : InteractableObject
{
    [Header("Overlay Settings")]
    public string correctPhotoId;
    public PhotoInventoryUI inventoryUI;

    [Header("Success Settings")]
    public GameObject photoDisplayObject;
    public DialogueSystem dialogueSystem;
    public List<DialogueLine> successDialogue;

    [Header("Portal Settings")]
    public GameObject portalObject;
    public Collider wallCollider;
    public string targetSceneName = "Id-4";
    public float fadeDuration = 1f;

    private bool puzzleSolved = false;
    private bool photoDisplayed = false;
    private Transform playerTransform;

    protected override void Start()
    {
        base.Start();
        interactType = InteractableType.Overlay;

        if (photoDisplayObject != null)
            photoDisplayObject.SetActive(false);
        if (portalObject != null)
            portalObject.SetActive(true);
        if (wallCollider != null)
            wallCollider.enabled = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    public override void OnInteract()
    {
        if (puzzleSolved)
        {
            Debug.Log("[覆盖] 谜题已解开，照片已显示，请穿过墙壁");
            return;
        }

        base.OnInteract();
        ShowOverlay();
    }

    void ShowOverlay()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<PhotoInventoryUI>();
        }

        if (inventoryUI != null)
        {
            PhoneManager.Instance.OpenInventory();
            inventoryUI.ShowInventory(this);
            Debug.Log("[覆盖] 打开照片背包");
        }
        else
        {
            Debug.LogWarning("[覆盖] 未找到 PhotoInventoryUI！");
        }
    }

    public void OnPhotoSelected(PhotoData photo)
    {
        bool success = (photo.photoId == correctPhotoId);

        if (inventoryUI != null)
        {
            inventoryUI.HideInventory();
        }
        PhoneManager.Instance.CloseInventory();

        if (success)
        {
            Debug.Log("[覆盖] 选择正确！谜题解开！");
            puzzleSolved = true;
            OnPuzzleSolved(photo);
        }
        else
        {
            Debug.Log("[覆盖] 选择错误，请重试");
        }
    }

    void OnPuzzleSolved(PhotoData photo)
    {
        if (photoDisplayObject != null)
        {
            MeshRenderer meshRenderer = photoDisplayObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null && photo.photoSprite != null)
            {
                Material mat = meshRenderer.material;
                if (mat != null)
                {
                    mat.mainTexture = photo.photoSprite.texture;
                }
            }
            photoDisplayObject.SetActive(true);
            photoDisplayed = true;
            Debug.Log("[覆盖] 照片已显示在墙上");
        }

        if (wallCollider != null)
        {
            wallCollider.enabled = false;
            Debug.Log("[覆盖] 墙壁已可穿越");
        }

        if (dialogueSystem != null && successDialogue != null && successDialogue.Count > 0)
        {
            dialogueSystem.ShowDialogue(successDialogue, OnDialogueComplete);
        }
        else
        {
            Debug.Log("[对话] 模拟对话: 这面墙可以穿过去了！");
            OnDialogueComplete();
        }
    }

    void OnDialogueComplete()
    {
        Debug.Log("[游戏] 对话结束！请穿过墙壁进入下一关");
    }

    void Update()
    {
        if (puzzleSolved && playerTransform != null && wallCollider != null)
        {
            float distanceToWall = Vector3.Distance(playerTransform.position, wallCollider.transform.position);
            if (distanceToWall < 1.5f)
            {
                Vector3 wallForward = wallCollider.transform.forward;
                Vector3 playerToWall = playerTransform.position - wallCollider.transform.position;
                float dotProduct = Vector3.Dot(wallForward.normalized, playerToWall.normalized);
                
                if (dotProduct < 0)
                {
                    Debug.Log("[传送门] 玩家已穿过墙壁！正在切换场景...");
                    EnterPortal();
                }
            }
        }
    }

    void EnterPortal()
    {
        Debug.Log($"[传送门] 正在切换到场景: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
}

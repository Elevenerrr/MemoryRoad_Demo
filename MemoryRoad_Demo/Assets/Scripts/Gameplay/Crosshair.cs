using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public RectTransform crosshairImage;
    public float size = 900f;
    public Color normalColor = Color.white;
    public Color interactColor = Color.green;
    public Color invalidColor = Color.red;

    private Camera mainCamera;
    private Image crosshairImg;

    void Start()
    {
        mainCamera = Camera.main;
        if (crosshairImage != null)
        {
            crosshairImg = crosshairImage.GetComponent<Image>();
        }
    }

    void Update()
    {
        UpdateCrosshair();
    }

    void UpdateCrosshair()
    {
        if (crosshairImage == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                SetCrosshairColor(interactColor);
            }
            else
            {
                SetCrosshairColor(invalidColor);
            }
        }
        else
        {
            SetCrosshairColor(normalColor);
        }
    }

    void SetCrosshairColor(Color color)
    {
        if (crosshairImg != null)
        {
            crosshairImg.color = color;
        }
    }
}

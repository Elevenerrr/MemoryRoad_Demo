using UnityEngine;
using System.Collections.Generic;

public class WaterGroundOutlineGenerator : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = new Color(0.3f, 0.8f, 1.0f, 0.8f);
    public float outlineWidth = 0.08f;
    public float outlineOffset = 0.05f;

    [Header("References")]
    public Transform groundsParent;
    public GameObject waterSurface;

    private List<LineRenderer> outlineRenderers = new List<LineRenderer>();

    private void Start()
    {
        if (groundsParent == null)
        {
            groundsParent = transform;
        }

        GenerateOutline();
    }

    private void OnValidate()
    {
        UpdateExistingLines();
    }

    [ContextMenu("Generate Outline")]
    public void GenerateOutline()
    {
        ClearExistingLines();

        if (waterSurface == null)
        {
            Debug.LogWarning("WaterSurface is not assigned!");
            return;
        }

        List<Transform> cubes = GetAllChildCubes();
        if (cubes.Count == 0)
        {
            Debug.LogWarning("No child cubes found in Grounds");
            return;
        }

        Bounds bounds = CalculateBounds(cubes);
        float waterY = waterSurface.transform.position.y;

        CreateWaterSurfaceOutline(bounds, waterY);

        Debug.Log("Water-Ground outline generated successfully!");
    }

    [ContextMenu("Update Outline Colors")]
    public void UpdateExistingLines()
    {
        foreach (LineRenderer lr in outlineRenderers)
        {
            if (lr != null)
            {
                lr.startColor = outlineColor;
                lr.endColor = outlineColor;
                lr.startWidth = outlineWidth;
                lr.endWidth = outlineWidth;

                if (lr.material != null)
                {
                    lr.material.color = outlineColor;
                }
            }
        }
    }

    private List<Transform> GetAllChildCubes()
    {
        List<Transform> cubes = new List<Transform>();
        foreach (Transform child in groundsParent)
        {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null)
            {
                cubes.Add(child);
            }
        }
        return cubes;
    }

    private Bounds CalculateBounds(List<Transform> cubes)
    {
        Bounds bounds = new Bounds(cubes[0].position, Vector3.zero);
        foreach (Transform cube in cubes)
        {
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            else
            {
                bounds.Encapsulate(cube.position);
            }
        }
        return bounds;
    }

    private void CreateWaterSurfaceOutline(Bounds bounds, float waterY)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3 frontLeft = new Vector3(center.x - extents.x, waterY, center.z - extents.z);
        Vector3 frontRight = new Vector3(center.x + extents.x, waterY, center.z - extents.z);
        Vector3 backRight = new Vector3(center.x + extents.x, waterY, center.z + extents.z);
        Vector3 backLeft = new Vector3(center.x - extents.x, waterY, center.z + extents.z);

        CreateOutlineLine(frontLeft, frontRight, "WaterEdge_Front", Vector3.back);
        CreateOutlineLine(frontRight, backRight, "WaterEdge_Right", Vector3.right);
        CreateOutlineLine(backRight, backLeft, "WaterEdge_Back", Vector3.forward);
        CreateOutlineLine(backLeft, frontLeft, "WaterEdge_Left", Vector3.left);
    }

    private void CreateOutlineLine(Vector3 start, Vector3 end, string name, Vector3 offsetDirection)
    {
        Vector3 offset = offsetDirection * outlineOffset;

        GameObject lineObj = new GameObject(name);
        lineObj.transform.parent = transform;
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startColor = outlineColor;
        lineRenderer.endColor = outlineColor;
        lineRenderer.startWidth = outlineWidth;
        lineRenderer.endWidth = outlineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start + offset);
        lineRenderer.SetPosition(1, end + offset);

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = outlineColor;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.useWorldSpace = true;

        outlineRenderers.Add(lineRenderer);
    }

    private void ClearExistingLines()
    {
        foreach (LineRenderer lr in outlineRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        outlineRenderers.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundsParent == null || waterSurface == null) return;

        List<Transform> cubes = GetAllChildCubes();
        if (cubes.Count == 0) return;

        Bounds bounds = CalculateBounds(cubes);
        float waterY = waterSurface.transform.position.y;

        Gizmos.color = Color.cyan;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3 frontLeft = new Vector3(center.x - extents.x, waterY, center.z - extents.z);
        Vector3 frontRight = new Vector3(center.x + extents.x, waterY, center.z - extents.z);
        Vector3 backRight = new Vector3(center.x + extents.x, waterY, center.z + extents.z);
        Vector3 backLeft = new Vector3(center.x - extents.x, waterY, center.z + extents.z);

        Gizmos.DrawLine(frontLeft, frontRight);
        Gizmos.DrawLine(frontRight, backRight);
        Gizmos.DrawLine(backRight, backLeft);
        Gizmos.DrawLine(backLeft, frontLeft);
    }

    private void OnDestroy()
    {
        ClearExistingLines();
    }
}

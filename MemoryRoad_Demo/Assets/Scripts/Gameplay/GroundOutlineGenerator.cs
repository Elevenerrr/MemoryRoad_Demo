using UnityEngine;
using System.Collections.Generic;

public class GroundOutlineGenerator : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color topEdgeColor = new Color(0.2f, 0.6f, 1.0f, 1.0f);
    public Color cornerColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);
    public float topEdgeWidth = 0.1f;
    public float cornerLineThickness = 0.15f;

    [Header("References")]
    public Transform groundsParent;
    public Material outlineMaterial;

    private List<LineRenderer> topEdgeRenderers = new List<LineRenderer>();
    private List<LineRenderer> cornerRenderers = new List<LineRenderer>();

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

        List<Transform> cubes = GetAllChildCubes();
        if (cubes.Count == 0)
        {
            Debug.LogWarning("No child cubes found in Grounds");
            return;
        }

        Bounds bounds = CalculateBounds(cubes);
        CreateTopSurfaceOutline(bounds);
        CreateCornerVerticalLines(bounds);

        Debug.Log("Outline generated successfully!");
    }

    [ContextMenu("Update Outline Colors")]
    public void UpdateExistingLines()
    {
        foreach (LineRenderer lr in topEdgeRenderers)
        {
            if (lr != null)
            {
                lr.startColor = topEdgeColor;
                lr.endColor = topEdgeColor;
                lr.startWidth = topEdgeWidth;
                lr.endWidth = topEdgeWidth;

                // Also update material color
                if (lr.material != null)
                {
                    lr.material.color = topEdgeColor;
                }
            }
        }

        foreach (LineRenderer lr in cornerRenderers)
        {
            if (lr != null)
            {
                lr.startColor = cornerColor;
                lr.endColor = cornerColor;
                lr.startWidth = cornerLineThickness;
                lr.endWidth = cornerLineThickness;

                // Also update material color
                if (lr.material != null)
                {
                    lr.material.color = cornerColor;
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

    private void CreateTopSurfaceOutline(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3 topFrontLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
        Vector3 topFrontRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
        Vector3 topBackRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
        Vector3 topBackLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);

        Vector3[] topEdges = {
            topFrontLeft, topFrontRight,
            topFrontRight, topBackRight,
            topBackRight, topBackLeft,
            topBackLeft, topFrontLeft
        };

        for (int i = 0; i < topEdges.Length; i += 2)
        {
            CreateTopEdgeLine(topEdges[i], topEdges[i + 1], "TopEdge_" + (i / 2));
        }
    }

    private void CreateCornerVerticalLines(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3 topFrontLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
        Vector3 topFrontRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
        Vector3 topBackRight = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
        Vector3 topBackLeft = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);

        Vector3 bottomFrontLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
        Vector3 bottomFrontRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
        Vector3 bottomBackRight = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
        Vector3 bottomBackLeft = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);

        CreateCornerLine(topFrontLeft, bottomFrontLeft, "CornerLine_FrontLeft");
        CreateCornerLine(topFrontRight, bottomFrontRight, "CornerLine_FrontRight");
        CreateCornerLine(topBackRight, bottomBackRight, "CornerLine_BackRight");
        CreateCornerLine(topBackLeft, bottomBackLeft, "CornerLine_BackLeft");
    }

    private void CreateTopEdgeLine(Vector3 start, Vector3 end, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.parent = transform;
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startColor = topEdgeColor;
        lineRenderer.endColor = topEdgeColor;
        lineRenderer.startWidth = topEdgeWidth;
        lineRenderer.endWidth = topEdgeWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Use Sprites/Default material to allow color changes
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = topEdgeColor;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.useWorldSpace = true;

        topEdgeRenderers.Add(lineRenderer);
    }

    private void CreateCornerLine(Vector3 start, Vector3 end, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.parent = transform;
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startColor = cornerColor;
        lineRenderer.endColor = cornerColor;
        lineRenderer.startWidth = cornerLineThickness;
        lineRenderer.endWidth = cornerLineThickness;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Use Sprites/Default material to allow color changes
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = cornerColor;

        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.useWorldSpace = true;

        cornerRenderers.Add(lineRenderer);
    }

    private void ClearExistingLines()
    {
        foreach (LineRenderer lr in topEdgeRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        topEdgeRenderers.Clear();

        foreach (LineRenderer lr in cornerRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        cornerRenderers.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundsParent == null) return;

        List<Transform> cubes = GetAllChildCubes();
        if (cubes.Count == 0) return;

        Bounds bounds = CalculateBounds(cubes);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void OnDestroy()
    {
        ClearExistingLines();
    }
}
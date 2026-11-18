using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Building")]
    public BuildingSriptable buildingData;
    private GameObject objectToPlace;
    private Vector2Int buildingSize = new Vector2Int(1, 1); // width (X) and depth (Z) in cells
    private float yOffset = 0.5f;                           // half the building's height if pivot is centered
    public KeyCode rotateKey = KeyCode.R;

    [Header("Pivot Correction")]
    private float pivotOffsetX = 0f;
    private float pivotOffsetZ = 0f;

    [Header("Grid")]
    public float gridSize = 1f;
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float gridX = 0f;
    public float gridY = 0.01f;
    public float gridZ = 0f;

    [Header("Grid Lines")]
    public Material lineMaterial;
    public float lineWidth = 0.02f;

    private GameObject tempObject;
    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private int rotationSteps = 0; // 0..3, each step = 90 degrees
    private Vector2Int currentOrigin;
    private bool canPlace;
    private Vector2Int EffectiveSize =>
        (rotationSteps % 2 == 0) ? buildingSize : new Vector2Int(buildingSize.y, buildingSize.x);

    private void Awake()
    {
        objectToPlace = buildingData != null ? buildingData.buildingPrefab : objectToPlace;
        yOffset = buildingData != null ? buildingData.yOffset : yOffset;
        pivotOffsetX = buildingData != null ? buildingData.pivotOffsetX : pivotOffsetX;
        pivotOffsetZ = buildingData != null ? buildingData.pivotOffsetZ : pivotOffsetZ;
        buildingSize = buildingData != null ? buildingData.size : buildingSize;

    }
    private void Start()
    {
        CreateTempObject();
        DrawGrid();
        rotationSteps = (rotationSteps + 1) % 4;
        tempObject.transform.rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(rotateKey))
        {
            rotationSteps = (rotationSteps + 1) % 4;
            tempObject.transform.rotation = Quaternion.Euler(0f, rotationSteps * 90f, 0f);
        }

        UpdateTempPosition();

        if (Input.GetMouseButtonDown(0) && canPlace)
            PlaceObject();
    }

    private void UpdateTempPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Vector2Int size = EffectiveSize;

        int cellX = Mathf.FloorToInt((hit.point.x - gridX) / gridSize);
        int cellZ = Mathf.FloorToInt((hit.point.z - gridZ) / gridSize);

        currentOrigin = new Vector2Int(cellX - size.x / 2, cellZ - size.y / 2);

        tempObject.transform.position = GetPlacementPosition(currentOrigin, size);

        canPlace = IsAreaFree(currentOrigin, size);
        SetTempObjectColor(canPlace ? Color.green : Color.red);
    }

    private Vector3 GetPlacementPosition(Vector2Int origin, Vector2Int size)
    {
        Vector3 center = new Vector3(
            gridX + (origin.x + size.x / 2f) * gridSize,
            yOffset,
            gridZ + (origin.y + size.y / 2f) * gridSize
        );

        Quaternion rot = Quaternion.Euler(0f, rotationSteps * 90f, 0f);
        Vector3 offset = rot * new Vector3(pivotOffsetX, 0f, pivotOffsetZ);

        return center - offset;
    }

    private bool IsAreaFree(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int cell = new Vector2Int(origin.x + x, origin.y + z);

                bool outOfBounds = cell.x < 0 || cell.x >= gridWidth ||
                                   cell.y < 0 || cell.y >= gridHeight;

                if (outOfBounds || occupiedCells.Contains(cell))
                    return false;
            }
        }
        return true;
    }

    private void PlaceObject()
    {
        Vector2Int size = EffectiveSize;

        Instantiate(objectToPlace, tempObject.transform.position, tempObject.transform.rotation);

        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                occupiedCells.Add(new Vector2Int(currentOrigin.x + x, currentOrigin.y + z));
    }

    private void CreateTempObject()
    {
        tempObject = Instantiate(objectToPlace);

        foreach (Collider c in tempObject.GetComponentsInChildren<Collider>())
            c.enabled = false;

        foreach (Renderer renderer in tempObject.GetComponentsInChildren<Renderer>())
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = 0.5f;
            mat.color = color;

            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    private void SetTempObjectColor(Color color)
    {
        foreach (Renderer renderer in tempObject.GetComponentsInChildren<Renderer>())
            renderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
    }
    private void DrawGrid()
    {
        GameObject gridParent = new GameObject("Grid Lines");
        gridParent.transform.SetParent(transform);

        for (int x = 0; x <= gridWidth; x++)
            CreateLine(gridParent.transform,
                new Vector3(gridX + x * gridSize, gridY, gridZ),
                new Vector3(gridX + x * gridSize, gridY, gridZ + gridHeight * gridSize));

        for (int z = 0; z <= gridHeight; z++)
            CreateLine(gridParent.transform,
                new Vector3(gridX, gridY, gridZ + z * gridSize),
                new Vector3(gridX + gridWidth * gridSize, gridY, gridZ + z * gridSize));
    }

    private void CreateLine(Transform parent, Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(parent);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}

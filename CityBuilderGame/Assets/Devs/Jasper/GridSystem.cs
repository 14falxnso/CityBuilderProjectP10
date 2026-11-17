using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1f;
    private GameObject tempObject;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();

    public Material lineMaterial;
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float gridX = 0f;
    public float gridY = 0.01f;
    public float gridZ = 0f;
    public float lineWidth = 0.02f;

    private void Start()
    {
        CreateTempObject();
        DrawGrid();
    }

    private void Update()
    {
        UpdateTempPosition();
        if (Input.GetMouseButtonDown(0))
        {
            PlaceObject();
        }
    }

    void CreateTempObject()
    {
        tempObject = Instantiate(objectToPlace);
        tempObject.GetComponent<Collider>().enabled = false;

        Renderer[] renderers = tempObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
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

    private void UpdateTempPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 gridPosition = new Vector3(
                Mathf.Floor((hit.point.x - gridX) / gridSize) * gridSize + gridSize / 2f + gridX,
                gridSize / 2f,
                Mathf.Floor((hit.point.z - gridZ) / gridSize) * gridSize + gridSize / 2f + gridZ
            );
            tempObject.transform.position = gridPosition;

            if (occupiedPositions.Contains(gridPosition))
                SetTempObjectColor(Color.red);
            else
                SetTempObjectColor(Color.green);
        }
    }

    private void SetTempObjectColor(Color color)
    {
        Renderer[] renderers = tempObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            mat.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }

    void PlaceObject()
    {
        Vector3 position = tempObject.transform.position;
        if (!occupiedPositions.Contains(position))
        {
            Instantiate(objectToPlace, position, Quaternion.identity);
            occupiedPositions.Add(position);
        }
    }

    void DrawGrid()
    {
        GameObject gridParent = new GameObject("Grid Lines");
        gridParent.transform.SetParent(transform);

        for (int x = 0; x <= gridWidth; x++)
        {
            CreateLine(gridParent.transform,
                new Vector3(gridX + x * gridSize, gridY, gridZ),
                new Vector3(gridX + x * gridSize, gridY, gridZ + gridHeight * gridSize));
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            CreateLine(gridParent.transform,
                new Vector3(gridX, gridY, gridZ + z * gridSize),
                new Vector3(gridX + gridWidth * gridSize, gridY, gridZ + z * gridSize));
        }
    }

    void CreateLine(Transform parent, Vector3 start, Vector3 end)
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
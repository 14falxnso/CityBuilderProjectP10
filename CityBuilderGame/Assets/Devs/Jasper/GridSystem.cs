using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1f;
    private GameObject tempObject;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();


    private void Start()
    {
        CreateTempObject();
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

            mat.SetFloat("_Mode", 2); // Set to transparent mode
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
                Mathf.Round(hit.point.x / gridSize) * gridSize,
                Mathf.Round(hit.point.y / gridSize) * gridSize,
                Mathf.Round(hit.point.z / gridSize) * gridSize
            );
            tempObject.transform.position = gridPosition;

            if (occupiedPositions.Contains(gridPosition))
            {
                SetTempObjectColor(Color.red);
            }
            else
            {
                SetTempObjectColor(Color.green);
            }
        }

    }

    private void SetTempObjectColor(Color color)
    {
        Renderer[] renderers = tempObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            Color currentColor = mat.color;
            currentColor.a = 0.5f; // Keep the alpha value at 0.5
            mat.color = color;
            mat.color = new Color(color.r, color.g, color.b, 0.5f); // Set the alpha to 0.5
        }
    }

    void PlaceObject()
    {
        Vector3 position = tempObject.transform.position;
        if (!occupiedPositions.Contains(position))
        {
            GameObject placedObject = Instantiate(objectToPlace, position, Quaternion.identity);
            occupiedPositions.Add(position);
        }
    }
}

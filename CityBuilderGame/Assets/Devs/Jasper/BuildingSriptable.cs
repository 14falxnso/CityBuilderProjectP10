using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Building/Create New Building")]
public class BuildingSriptable : ScriptableObject
{
    public GameObject buildingPrefab;
    public Vector2Int size = Vector2Int.one;
    public float yOffset = 0.5f;
    public float pivotOffsetX = 0f;
    public float pivotOffsetZ = 0f;

}

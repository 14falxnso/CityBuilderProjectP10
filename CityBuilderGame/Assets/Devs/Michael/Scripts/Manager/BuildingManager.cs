using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [Header("Gebouw Menus")]
    [SerializeField] private GameObject buildingMenuUI;
    [SerializeField] private Button buildingMenuButton;

    [Header("Script Links")]
    [SerializeField] private GridSystem gridSystemScript;

    private void Start()
    {
        buildingMenuUI.SetActive(false);
        gridSystemScript.BuildingModeActive = false;
    }
    private void Awake()
    {
        buildingMenuButton.onClick.AddListener(() => OpenBuildingMenu()); // knoppie klikken

    }


    private void OpenBuildingMenu() // menu openen nee?
    {
        if (!buildingMenuUI.activeSelf)
        {
            buildingMenuUI.SetActive(true);
            gridSystemScript.BuildingModeActive = true;
        }
        else
        {
            buildingMenuUI.SetActive(false);
            gridSystemScript.BuildingModeActive = false;
        }
    }
}

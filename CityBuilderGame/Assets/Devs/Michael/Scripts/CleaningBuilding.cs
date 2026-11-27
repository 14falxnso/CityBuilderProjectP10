using UnityEngine;

public class CleaningBuilding : BuildingBase
{
    public override void BuildingUse()
    {
        // Implement the specific logic for the cleaning building's use
        Debug.Log("Cleaning building is being used!");
        // Add cleaning logic here, such as cleaning in radius
    }

    public override void OnBuildingClicked()
    {
        base.OnBuildingClicked();
        // Additional logic specific to the cleaning building when clicked
        Debug.Log("Cleaning building clicked!");
    }

    public override void OnBuildingPlaced()
    {
        base.OnBuildingPlaced();
        // Additional logic specific to the fishing building when placed
        Debug.Log("Cleaning building placed!");
    }

    public override void OnBuildingMoved()
    {
        base.OnBuildingMoved();
        // Additional logic specific to the cleaning building when moved
        Debug.Log("Cleaning building moved!");
    }

    public override void OnBuildingUpgraded()
    {
        base.OnBuildingUpgraded();
        // Additional logic specific to the cleaning building when upgraded
        Debug.Log("Cleaning building upgraded!");
    }

    private void Generate
}

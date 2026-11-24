using UnityEngine;

public class FishingBuilding : BuildingBase
{
    public override void BuildingUse()
    {
        // Implement the specific logic for the fishing building's use
        Debug.Log("Fishing building is being used!");
        // Add fishing logic here, such as catching fish or generating resources
    }

    public override void OnBuildingClicked()
    {
        base.OnBuildingClicked();
        // Additional logic specific to the fishing building when clicked
        Debug.Log("Fishing building clicked!");
    }

    public override void OnBuildingPlaced()
    {
        base.OnBuildingPlaced();
        // Additional logic specific to the fishing building when placed
        Debug.Log("Fishing building placed!");
    }

    public override void OnBuildingMoved()
    {
        base.OnBuildingMoved();
        // Additional logic specific to the fishing building when moved
        Debug.Log("Fishing building moved!");
    }

    public override void OnBuildingUpgraded()
    {
        base.OnBuildingUpgraded();
        // Additional logic specific to the fishing building when upgraded
        Debug.Log("Fishing building upgraded!");
    }
}

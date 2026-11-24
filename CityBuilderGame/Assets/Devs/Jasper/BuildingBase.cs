using System.Collections;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public int TimeBetweenActivations = 10;
    private int currentTimer = 0;

    [Range(0, 100)]
    private int currentEfficiency = 0;
    private void Start()
    {
        StartCoroutine(DelayAction());
    }

    virtual public void OnBuildingClicked()
    {
        //Open the building/upgrade menu
        //Highlight the building in some way
        //Be able to see effieciency

    }

    virtual public void OnBuildingPlaced()
    {
        //Audio effect ofzo?
        //Koele animatie ofzo idk?
    }

    virtual public void OnBuildingMoved()
    {
        //Audio effect ofzo?
        //Koele animatie ofzo idk?
    }

    virtual public void BuildingUse()
    {
        //Reepat the benefits of the building, like catching fih or something
        //Sounds, animations, etc
    }

    virtual public void OnBuildingUpgraded()
    {
        //Upgrade the building, make it more efficient, etc
        //Sounds, animations, etc
    }

    IEnumerator DelayAction()
    {
        while (true)
        {
            BuildingUse();
            yield return new WaitForSeconds(TimeBetweenActivations / (currentEfficiency / 100)); //This will make the building use faster if the efficiency is higher
        }
    }
}





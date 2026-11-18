using TMPro;
using UnityEngine;

public class PurchaseBuilding : MonoBehaviour
{
    [Header("UI Info")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private Texture2D buildingTexture;

    [SerializeField] private Texture2D lockTexture;

    [Header("Building Info")]
    [SerializeField] private TextMeshProUGUI dataText;
    [SerializeField] private GameObject buildingObject;

    [Header("Costs")]
    [SerializeField] private int currency1;
    [SerializeField] private int currency2;
    [SerializeField] private int currency3;
    

    // player inventory linken

    private void BuyBuilding() // moet op de button komen
    {
        // tijdelijke resources -> moet later de player resources worden
        int testCurrency1 = 100;
        int testCurrency2 = 100;
        int testCurrency3 = 100;

        if (testCurrency1 >= currency1 | testCurrency2 >= currency2 | testCurrency3 >= currency3 ) 
        {
            testCurrency1 -= currency1;
            testCurrency2 -= currency2;
            testCurrency3 -= currency3;

            AddBuildingToPlayerInventory();
        }
    }

    private void AddBuildingToPlayerInventory()
    {

    }

}

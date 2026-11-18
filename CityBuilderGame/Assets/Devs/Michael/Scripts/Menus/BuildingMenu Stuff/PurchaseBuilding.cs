using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseBuilding : MonoBehaviour
{
    [Header("UI Info")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private string buildingName;
    [SerializeField] private Texture2D buildingTexture;

    [SerializeField] private GameObject lockImage;

    [Header("Building Info")]
    [SerializeField] private TextMeshProUGUI dataText;
    [SerializeField] private BuildingSriptable buildingObject;

    [Header("Costs")]
    [SerializeField] private int buildingLevel;

    [SerializeField] private int currency1;
    [SerializeField] private int currency2;
    [SerializeField] private int currency3;


    // player inventory linken

    private void Start()
    {
        lockImage.SetActive(true);
    }

    private void Update()
    {
        UpdateText();
    }

    private void BuyBuilding() // moet op de button komen
    {

        // tijdelijke resources -> moet later de player resources worden
        int testCurrency1 = 100;
        int testCurrency2 = 100;
        int testCurrency3 = 100;

        if (testCurrency1 >= currency1 | testCurrency2 >= currency2 | testCurrency3 >= currency3 ) // i a o dusss dit moet ja ook vervangen wortden
        {
            testCurrency1 -= currency1;
            testCurrency2 -= currency2;
            testCurrency3 -= currency3;

            AddBuildingToPlayerInventory();
        }
        UpdateText();
    }

    private void RemoveLockImage() // de lock image verwijdeen
    {
        int testPlayerLevel = 100; // MOET OOK VERVANGEN WORDEN

        if (testPlayerLevel >= buildingLevel)
        {
            lockImage.SetActive(true);
        }
    }

    private void AddBuildingToPlayerInventory() // vervang de tijdelijke list door de player inventory
    {
        //List<BuildingSriptable> testInventory;
            
        //testInventory.Add( buildingObject ); 
    }

    private void UpdateText() // update de text 
    {
        buildingNameText.text = buildingName;
        dataText.text = currency1.ToString() + "|" + currency2.ToString() + "|" + currency3.ToString();
    }

}

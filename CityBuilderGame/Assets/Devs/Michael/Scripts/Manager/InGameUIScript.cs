using TMPro;
using UnityEngine;

public class InGameUIScript : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelAmountText;
    [SerializeField] private TextMeshProUGUI populationAmountText;
    [SerializeField] private TextMeshProUGUI stoneAmountText;
    [SerializeField] private TextMeshProUGUI bambooAmountText;
    [SerializeField] private TextMeshProUGUI bladAmountText;

    //[Header("Script link")]
    //[SerializeField] private playerscript player;

    private void UpdateText()
    {
        //levelAmountText.text = player.level;
        //populationAmountText.text = player.population;
        //stoneAmountText.text = player.stone;
        //bambooAmountText.text = player.bamboo;
        //bladAmountText.text = player.blad;
    }
}

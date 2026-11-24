using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

//kzit dit lowkey te erg uit te werken jaja
public class FishGameManager : MonoBehaviour
{
    public static FishGameManager Instance;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public float maxDepth = 100f;
    public bool isGameActive = true;

    [SerializeField] private float currentDepth;
    [SerializeField] private float depthPercentage;

    [SerializeField] private Material bgShader;
    [SerializeField] private Light sunLight;

    [SerializeField] private GameObject playField;

    public int maxFishCaught;
    [SerializeField] private int currentFishCaught;

    [Header("End / Stat screen info !!")]
    [SerializeField] private GameObject statScreenHolder;

    [SerializeField] private TextMeshProUGUI xpField;
    [SerializeField] private TextMeshProUGUI foodGainedField;
    [SerializeField] private TextMeshProUGUI energyUsedField;

    [SerializeField] private Button continueButton;
    

    [SerializeField] private EndGameFishStats endGameFishStats;
    public List<FishStats> fishCaughtStats = new List<FishStats>();

    [Header("wat coole deco dingen")]
    [SerializeField] private CanvasGroup tutorialGroup;

    private bool hasGameEndedAlready = false;

    private void Start() {
        StartCoroutine(DepthTimer());
        StartCoroutine(FadeOutTutorialTab());

        //laat de stat screen NIET zien (wel zo handig jaja)
        statScreenHolder.SetActive(false);

        endGameFishStats = new EndGameFishStats();
    }

    private IEnumerator DepthTimer()
    {
        for(currentDepth = maxDepth; currentDepth > 0; currentDepth--)
        {
            yield return new WaitForFixedUpdate();
            depthPercentage = 100 - 100 / maxDepth * currentDepth; 

            //bereken dit en dat enzo yeah yeah lets go
            bgShader.SetFloat("_HeightFactor", 0.9f - 0.9f / 100f * depthPercentage);
            sunLight.intensity = 2f / 100f * depthPercentage;

            //0.9 = donker, 0 = licht
        }

        yield return new WaitForSeconds(1f);

        StartCoroutine(EndFishingGame());
    }

    private IEnumerator EndFishingGame()
    {
        isGameActive = false;

        //bereken score etc
        //dit moet SOWIESO nog aangepast worden maarja

        // endGameFishStats.foodGained = currentFishCaught;
        // endGameFishStats.XPgained = currentFishCaught * 10;
        // endGameFishStats.energyUsed = 10;

        //w is totale gewicht (dus eten)
        //x is totale XP (jaja coole ass berekening einstein type shit)
        float w = 0f;
        float x = 0f;
        foreach(FishStats s in fishCaughtStats)
        {
            w += 1f + s.weight;
            x += (
                1f + s.weight * 1f + s.rarity 
            ) * 5f;
        }

        endGameFishStats.foodGained = MathF.Round(w, 2);
        endGameFishStats.XPgained = MathF.Round(x, 2);

        endGameFishStats.energyUsed = 10f;

        for(int i = 0; i < 200; i++)
        {
            yield return new WaitForFixedUpdate();
            playField.transform.position += Vector3.up * 10f;
        }

        yield return new WaitForSeconds(1.5f);

        ShowStatScreen(endGameFishStats);
    }

    public void FishCaught()
    {
        currentFishCaught++;

        if(currentFishCaught >= maxFishCaught)
        {
            StartCoroutine(EndFishingGame());
        }
    }

    public void ShowStatScreen(EndGameFishStats e)
    {
        //fixes weird bug jaja
        if(hasGameEndedAlready) return;
        hasGameEndedAlready = true;

        //laat hem WEL weer zien boeeem
        statScreenHolder.SetActive(true);

        //zet alle info goed jaja
        xpField.text = "+" + e.XPgained.ToString() + " XP";
        foodGainedField.text = "+" + e.foodGained.ToString() + " Food";
        energyUsedField.text = "-" + e.energyUsed.ToString() + " Energy used";

        StartCoroutine(StatScreenAnimation());

    }

    [SerializeField] private float timeBetweenTheCoolAnimation = 0.2f;
    //cool haha
    private IEnumerator StatScreenAnimation()
    {
        xpField.enabled = false;
        foodGainedField.enabled = false;
        energyUsedField.enabled = false;
        continueButton.enabled = false;

        yield return new WaitForSeconds(0.5f);

        //lowkey voeg hier coole sfx aan toe 
        //like hill climb racing type shit :eyes:

        xpField.enabled = true;
        yield return new WaitForSeconds(timeBetweenTheCoolAnimation);

        foodGainedField.enabled = true;
        yield return new WaitForSeconds(timeBetweenTheCoolAnimation);

        energyUsedField.enabled = true;
        yield return new WaitForSeconds(timeBetweenTheCoolAnimation);

        continueButton.enabled = true;
    }

    //zo cool
    //lowkey gewoon fade out maar is cool
    private IEnumerator FadeOutTutorialTab()
    {
        yield return new WaitForSeconds(0.75f);

        for(float i = 1; i > 0; i -= 0.05f)
        {
            tutorialGroup.alpha = i;
            yield return new WaitForFixedUpdate();
        }

        tutorialGroup.alpha = 0f;
    }
}

[System.Serializable]
public class EndGameFishStats
{
    public float XPgained;
    public float foodGained;
    public float energyUsed;
}

[System.Serializable]
public class FishStats
{
    public float weight;

    [Header("tussen 0 en 10")]
    public float rarity;
}

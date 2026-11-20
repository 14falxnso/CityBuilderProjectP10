using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

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


    private void Start() {
        StartCoroutine(DepthTimer());
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

        for(int i = 0; i < 200; i++)
        {
            yield return new WaitForFixedUpdate();
            playField.transform.position += Vector3.up * 10f;
        }
    }

    public void FishCaught()
    {
        currentFishCaught++;

        if(currentFishCaught >= maxFishCaught)
        {
            StartCoroutine(EndFishingGame());
        }
    }
}

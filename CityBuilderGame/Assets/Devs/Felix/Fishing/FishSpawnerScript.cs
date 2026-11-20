using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FishSpawnerScript : MonoBehaviour
{
    private Transform putFishHere;
    [SerializeField] private GameObject fishPrefab;

    public float[] spawnTimeRange = new float[2]{2f, 5f};

    private void Start() {
        putFishHere = transform;

        StartCoroutine(FishSpawner());
    }

    private void SpawnFish()
    {
        Instantiate(fishPrefab);
    }

    private IEnumerator FishSpawner()
    {
        while (true)
        {
            if (!FishGameManager.Instance.isGameActive)
            {
                yield break;
            }

            //yeahyeahyeah willekeurige delay
            float newDelay = Random.Range(spawnTimeRange[0], spawnTimeRange[1]);

            yield return new WaitForSecondsRealtime(newDelay);
            SpawnFish();
        }
    }
}

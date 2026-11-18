using System.Collections;
using UnityEngine;

public class FishSpawnerScript : MonoBehaviour
{
    private Transform putFishHere;
    [SerializeField] private GameObject fishPrefab;

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
            yield return new WaitForSecondsRealtime(0.5f);
            SpawnFish();
        }
    }
}

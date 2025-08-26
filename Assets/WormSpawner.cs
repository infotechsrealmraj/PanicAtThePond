using NUnit.Framework;
using UnityEngine;

public class WormSpawner : MonoBehaviour
{
    public GameObject wormPrefab, goldWormPrefab;
    public float spawnInterval = 3f;
    public float xRange = 8f;
    public float yRange = 4f;

    private bool canSpawn = true;

    public static WormSpawner instence;


    private void Awake()
    {
        if (instence == null)
        {
            instence = this;
        }
    }

    void Start()
    {
        InvokeRepeating("SpawnWorm", 1f, spawnInterval);
    }

    void SpawnWorm()
    {
        if (!canSpawn) return;

        float x = Random.Range(-xRange, xRange);
        float y = Random.Range(-yRange, 1);
        Vector2 pos = new Vector2(x, y);

            Instantiate(goldWormPrefab, pos, Quaternion.identity);
    }

    public void StopSpawning()
    {
        canSpawn = false;
    }

    public void DestroyAllWorms()
    {
        GameObject[] worms = GameObject.FindGameObjectsWithTag("Worm");
        foreach (GameObject worm in worms)
        {
            Destroy(worm);
        }

        GameObject[] goldTrouts = GameObject.FindGameObjectsWithTag("GoldTrout");
        foreach (GameObject trout in goldTrouts)
        {
            Destroy(trout);
        }
    }
}

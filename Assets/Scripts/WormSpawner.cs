using UnityEngine;
using FishNet.Object;

public class WormSpawner : NetworkBehaviour
{
    public GameObject wormPrefab, goldWormPrefab;
    public float spawnInterval = 3f;
    public float xRange = 8f;
    public float yRange = 4f;

    public bool canSpawn = true;

    public static WormSpawner instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        Debug.Log($"[WormSpawner] IsServer={IsServer}, IsClient={IsClient}, IsHost={IsHost}");

        if (IsServer)
        {
            Debug.Log("I M Server");
            Invoke("SpawnWorm",0f);
            Invoke("SpawnGoldWorm", Random.Range(5f, 10f));
        }
    }

    void SpawnWorm()
    {
        if (canSpawn)
        {
            float x = Random.Range(-xRange, xRange);
            float y = Random.Range(-yRange, yRange);
            Vector2 pos = new Vector2(x, y);

            GameObject worm = Instantiate(wormPrefab, pos, Quaternion.identity);
            if (IsServer)
                Spawn(worm);
        }

        Invoke("SpawnWorm", Random.Range(2, 5));

    }


    void SpawnGoldWorm()
    {
        if (!canSpawn) return;

        float x = Random.Range(-xRange, xRange);
        float y = Random.Range(-yRange, yRange);
        Vector2 pos = new Vector2(x, y);

        GameObject goldWorm = Instantiate(goldWormPrefab, pos, Quaternion.identity);
        Spawn(goldWorm);
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

        GameObject[] worms2 = GameObject.FindGameObjectsWithTag("Worm2");
        foreach (GameObject worm in worms2)
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

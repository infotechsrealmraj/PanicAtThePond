using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class WormSpawner : NetworkBehaviour
{
    public GameObject wormPrefab, goldWormPrefab;
    public float spawnInterval = 3f;
    public float xRange = 8f;
    public float yRange = 4f;

    internal bool canSpawn = true;

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
            InvokeRepeating("SpawnWorm", 1f, spawnInterval);
            Invoke("SpawnGoldWorm", Random.Range(5f, 10f));
        }
    }

    void SpawnWorm()
    {

        float x = Random.Range(-xRange, xRange);
        float y = Random.Range(-yRange, 0);
        Vector2 pos = new Vector2(x, y);

        GameObject worm = Instantiate(wormPrefab, pos, Quaternion.identity);
        if (IsServer)
            Spawn(worm); // यह NetworkBehaviour की built-in method है (ServerManager.Spawn का shorthand)
    }


    void SpawnGoldWorm()
    {
        if (!canSpawn) return;

        float x = Random.Range(-xRange, xRange);
        float y = Random.Range(-yRange, 0);
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

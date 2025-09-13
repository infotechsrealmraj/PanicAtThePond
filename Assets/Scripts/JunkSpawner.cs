using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object; // ✅ FishNet networking

public class JunkSpawner : NetworkBehaviour
{
    public GameObject[] junkPrefabs;
    public float minSpawnDelay = 10f;   // min wait
    public float maxSpawnDelay = 15f;  // max wait
    public float xRange = 8f;
    public float yRange = 4f;

    internal bool canSpawn = true;
    public static JunkSpawner instance;



    private List<GameObject> activeJunks = new List<GameObject>(); // track all junks

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public override void OnStartServer()
    {
        if (IsServer)
        {
            base.OnStartServer();
            StartCoroutine(SpawnLoop());
        }
    }

    IEnumerator SpawnLoop()
    {
        while (canSpawn)
        {
            float x = Random.Range(-xRange, xRange);
            float y = yRange;
            Vector2 pos = new Vector2(x, y);

            GameObject prefab = junkPrefabs[Random.Range(0, junkPrefabs.Length)];
            GameObject newJunk = Instantiate(prefab, pos, Quaternion.identity);

            Spawn(newJunk); // FishNet का network spawn

            activeJunks.Add(newJunk);

            Junk junkScript = newJunk.AddComponent<Junk>();
            junkScript.onDestroyed = () => { activeJunks.Remove(newJunk); };

            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }

    public void StopSpawning()
    {
        canSpawn = false;
    }

    public void DestroyAllJunks()
    {
        foreach (GameObject junk in activeJunks)
        {
            if (junk != null)
                Despawn(junk); // ✅ networked despawn (Destroy की बजाय)
        }
        activeJunks.Clear();
    }
}

// Helper script
public class Junk : NetworkBehaviour
{
    public System.Action onDestroyed;

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (onDestroyed != null)
            onDestroyed.Invoke();
    }
}

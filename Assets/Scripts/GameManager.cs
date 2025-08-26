using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Slider के लिए

public class GameManager : MonoBehaviour
{
    [Header("Player Setup")]
    public int totalPlayers = 7;
    public GameObject fishermanPrefab;
    public GameObject fishPrefab;

    [Header("Worm Settings")]
    public int baseWormMultiplier = 3;

    [Header("Fish Spawn Bounds")]
    public Vector2 minBounds = new Vector2(-8f, -4f);
    public Vector2 maxBounds = new Vector2(8f, 4f);

    [Header("Runtime Info")]
    public int fishermanWorms;
    public List<GameObject> fishes = new List<GameObject>();

    [Header("UI")]
    public Slider castingMeter; // Assign this in Inspector

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        SetupGame();
    }

    void SetupGame()
    {
        // Spawn Fisherman
        GameObject fisherman = Instantiate(fishermanPrefab, new Vector3(0f, 1.75f, 0f), Quaternion.identity);
        fisherman.name = "Fisherman";

        // Assign castingMeter to FishermanController
        FishermanController fc = fisherman.GetComponent<FishermanController>();
        if (fc != null)
        {
            fc.castingMeter = castingMeter;
        }
        else
        {
            Debug.LogWarning("FishermanController not found on FishermanPrefab!");
        }

        // Worm calculation
        int fishCount = totalPlayers - 1;
        fishermanWorms = fishCount * baseWormMultiplier;
        Debug.Log("Fisherman Worms: " + fishermanWorms);


        // Spawn Fish
        for (int i = 0; i < 1; i++)
        {
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
            fish.name = "Fish_" + (i + 1);
            fishes.Add(fish);
        }

        Debug.Log("Fish Spawned: " + fishes.Count);
    }
}

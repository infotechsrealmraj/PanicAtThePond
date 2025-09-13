using FishNet.Object;
using UnityEngine;

public class NetworkObjectGenerater : NetworkBehaviour
{

    public GameObject MashPhase;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(IsServer)
        {
            SpawnWorm();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnWorm()
    {
        GameObject mashPhase = Instantiate(MashPhase);
        Spawn(mashPhase);
    }
}

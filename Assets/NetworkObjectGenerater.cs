using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class NetworkObjectGenerater : NetworkBehaviour
{

    public GameObject MashPhase;

    public Text clientText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(IsServer)
        {
            SpawnWorm();
                clientText.text  = "ClientId = " +  Owner.ClientId;
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

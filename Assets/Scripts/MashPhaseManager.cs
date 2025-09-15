using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MashPhaseManager : NetworkBehaviour
{
    public static MashPhaseManager instance;

    [Header("UI")]
    public GameObject mashPanel;
    public Slider mashSlider;   // 0 = escape (fish), 1 = capture (fisherman)
    public Text mashText;

    [Header("Settings")]
    public float mashSpeed = 0.01f;
    public float decayRate = 0.002f;

    private bool active = false;

    void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        GameManager gm = GameManager.instance;

        mashPanel = gm.mashPanel;
        mashSlider = gm.mashSlider;
        mashText = gm.mashText;
    }
    public void StartMashPhase()
    {
        if (IsServer)
        {
            ExecuteFunctionLocal();
            ExecuteFunctionObserversRpc();
        }
        else
        {
            CallUniversalServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CallUniversalServerRpc()
    {
        ExecuteFunctionLocal();

        ExecuteFunctionObserversRpc();
    }

    [ObserversRpc]
    private void ExecuteFunctionObserversRpc()
    {
        ExecuteFunctionLocal();
    }

    private void ExecuteFunctionLocal()
    {
        for (int i = 0; i < GameManager.instance.AllFishPlayers.Count; i++)
        {
            if (GameManager.instance.AllFishPlayers[i].isCatchedFish)
            {
                JunkSpawner.instance.canSpawn = WormSpawner.instance.canSpawn = FishermanController.instance.isCanMove = HungerSystem.instance.canDecrease = false;

                if (mashPanel != null) mashPanel.SetActive(true);
                if (mashSlider != null) mashSlider.value = 0f;

                active = true;
                mashText.text = "MASH SPACE BAR!";
                return;
            }
        }
    }
    void Update()
    {
        if (!active) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            mashSlider.value += mashSpeed * Time.deltaTime * 60;
        }

        if (mashSlider.value >= 1f)
        {
            mashSlider.value = 0;
            if (FishermanController.instance.isfisherMan)
            {
                EndMashPhase(true); // Fisherman caught
            }
            else
            {
                EndMashPhase(false); // Fisherman caught
            }
        }
    }

    // This is what you'll call from anywhere
    public void EndMashPhase(bool fishermanWon)
    {
        if (IsServer)
        {
            // Server pe ho to local call + broadcast to all clients
            EndMashPhaseLocal(fishermanWon);
            EndMashPhaseObserversRpc(fishermanWon);
        }
        else
        {
            // Client se server ko request
            EndMashPhaseServerRpc(fishermanWon);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndMashPhaseServerRpc(bool fishermanWon)
    {
        // Server executes + informs clients
        EndMashPhaseLocal(fishermanWon);
        EndMashPhaseObserversRpc(fishermanWon);
    }

    [ObserversRpc]
    private void EndMashPhaseObserversRpc(bool fishermanWon)
    {
        // Clients execute local logic
        if (!IsServer) // avoid double execution on host
            EndMashPhaseLocal(fishermanWon);
    }

    private void EndMashPhaseLocal(bool fishermanWon)
    {
        active = false;
        if (mashPanel != null) mashPanel.SetActive(false);

        if (fishermanWon)
        {   
            FishermanController.instance.catchadFishes++;
            Debug.Log("Fish won the mash phase! Escaped hook.");
            Transform myfish2 =  GameObject.FindGameObjectWithTag("CatchdFish").GetComponent<Transform>();
            myfish2.transform.SetParent(Hook.instance.wormParent.transform);
            myfish2.transform.localPosition = Vector3.zero;
            myfish2.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);

        }
        else
        {
            JunkSpawner.instance.canSpawn =
           WormSpawner.instance.canSpawn =
           FishermanController.instance.isCanMove =
           HungerSystem.instance.canDecrease =
           GameManager.instance.myFish.canMove = true;
            HungerSystem.instance.AddHunger(75f);
            Debug.Log("Fisherman won the mash phase! Caught fish.");

            for (int i = 0; i < GameManager.instance.AllFishPlayers.Count; i++)
            {
                if (GameManager.instance.AllFishPlayers[i] != null)
                {
                    GameManager.instance.AllFishPlayers[i].ChangeTag("Fish");
                }
                else
                {
                    Debug.Log("FishController is null");
                }
            }
        }

        if (IsServer)
        {
            Hook.instance.LoadReturnToRod();
        }
    }

    public void CatchFish(int clentID)
    {
        if (IsServer)
        {   
            // Server pe ho to local call + broadcast to all clients
            CatchFishLocal(clentID);
            CatchFishObserversRpc(clentID);
        }
        else
        {
            // Client se server ko request
            CatchFishServerRpc(clentID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CatchFishServerRpc(int clentID)
    {
        // Server executes + informs clients
        CatchFishLocal(clentID);
        CatchFishObserversRpc(clentID);
    }

    [ObserversRpc]
    private void CatchFishObserversRpc(int clentID)
    {
        // Clients execute local logic
        if (!IsServer) // avoid double execution on host
            CatchFishLocal(clentID);
    }

    public void CatchFishLocal(int r)
    {
        if (!FishermanController.instance.isfisherMan)
        {
            Transform myFish = GameManager.instance.myFish.transform;
            myFish.transform.SetParent(Hook.instance.wormParent.transform);
            myFish.transform.localPosition = Vector3.zero;
            myFish.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            if(IsOwner)
            {
                FishermanController.instance.catchedFish = GameManager.instance.myFish;
            }
        }
    }
}

using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishermanController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Rod Selection")]
    public Transform leftRod;
    public Transform rightRod;
    public Transform currentRod;

    [Header("Casting")]
    public KeyCode castKey1 = KeyCode.X;
    public KeyCode castKey2 = KeyCode.V;
    public Slider castingMeter;       // UI Slider (0-1)
    public float meterSpeed = 2f;
    public float maxCastDistance = 10f; // max distance hook can go

    [Header("Worms")]
    internal int worms ;

    [Header("Hook")]
    public GameObject hookPrefab;

    internal bool isCasting = false;
    internal bool isCanMove = true;
    internal bool isfisherMan = false;
    private bool meterIncreasing = true;

    [HideInInspector] public GameObject leftHook = null;
    [HideInInspector] public GameObject rightHook = null;

    [Header("Horizontal Bounds")]
    public float minX = -8f;
    public float maxX = 8f;

    public static FishermanController instance;

   
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    void Start()
    {
        currentRod = leftRod;
        if (castingMeter != null)
            castingMeter.value = 0;
    }

    void Update()
    {
        if (isfisherMan)
        {
            HandleMovement();
            HandleRodSelection();
            HandleCasting();
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        // ✅ Client पर spawn होने के बाद GameManager में assign करो
        if (IsOwner || IsClient)
        {
            GameManager.instance.AssignFisherman(this);
        }
    }

    void HandleMovement()
    {
        if (leftHook == null && rightHook == null && !isCasting && isCanMove)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                Debug.Log($"Sending input to server: {moveInput}");
                SendMoveInputServerRpc(moveInput, Time.fixedDeltaTime);
            }
        }
    }


    

    [ServerRpc(RequireOwnership = false)]
    private void SendMoveInputServerRpc(float moveInput, float deltaTime)
    {
        Debug.Log($"[SERVER] Received input: {moveInput}");
        Vector3 move = new Vector3(moveInput * moveSpeed * deltaTime, 0, 0);
        transform.position += move;

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        transform.position = clampedPos;
    }

    void HandleRodSelection()
    {
        if (Input.GetKeyDown(KeyCode.W))
            currentRod = leftRod;
        else if (Input.GetKeyDown(KeyCode.S))
            currentRod = rightRod;
    }

    void HandleCasting()
    {
        // X + V held down → start casting meter
        if (!isCasting && Input.GetKey(castKey1) && Input.GetKey(castKey2))
        {
            if (( leftHook != null) || (rightHook != null))
            {
                Debug.Log("Rod already has a hook!");
                return;
            }


            isCasting = true;
            StartCoroutine(CastMeterRoutine());

        }

        // Release → cast hook with meter value
        if (worms > 0)
        {
            if (isCasting && (!Input.GetKey(castKey1) || !Input.GetKey(castKey2)))
            {
                ReleaseCast();
            }
        }
    }
   

    IEnumerator CastMeterRoutine()
    {
        while (isCasting)
        {
            if (meterIncreasing)
            {
                castingMeter.value += Time.deltaTime * meterSpeed;
                if (castingMeter.value >= 1f) meterIncreasing = false;
            }
            else
            {
                castingMeter.value -= Time.deltaTime * meterSpeed;
                if (castingMeter.value <= 0f) meterIncreasing = true;
            }
            yield return null;
        }
    }

    void ReleaseCast()
    {
        isCanMove =  isCasting = false;
        StartCoroutine(CastMeterRoutine());

        if (hookPrefab != null && currentRod != null)
        {
            // Client -> Server ko request bheje
            float castDistance = castingMeter.value * maxCastDistance;
            ReleaseCastServerRpc(currentRod == leftRod, castDistance);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseCastServerRpc(bool isLeftRod, float castDistance)
    {
        Transform rod = isLeftRod ? leftRod : currentRod;

        GameObject hook = Instantiate(hookPrefab, rod.position, Quaternion.identity);
        hook.name = "Hook";

        Hook hookScript = hook.GetComponent<Hook>();
        if (hookScript != null)
        {
            hookScript.rodTip = rod;
            hookScript.LaunchDownWithDistance(castDistance);
        }

        // Track hook per rod
        if (isLeftRod) leftHook = hook;
        else rightHook = hook;

        // Server spawn karega
        Spawn(hook);

        // Worm count decrease
        if (worms > 0)
        {
            worms--;
            GameManager.instance.UpdateUI(worms);
            Debug.Log($"[SERVER] Worm used! Remaining: {worms}");
        }
    }

    public void ClearHookReference(GameObject hook)
    {
        if (hook == leftHook) leftHook = null;
        if (hook == rightHook) rightHook = null;
    }


    [ObserversRpc]
    public void SetCurruntRodinHook(Transform tip)
    {
        Debug.Log("SetCurruntRodinHook");
        Hook.instance.rodTip = tip;
    }

    // Check worms and print lose message
    public void CheckWorms()
    {
        if (isfisherMan)
        {

            if (worms <= 0)
            {
                if (GameManager.instance != null && GameManager.instance.gameOverText != null)
                {
                    GameManager.instance.ShowGameOver("Fisherman Lose!\nFishes Win!");
                }
                WormSpawner.instance.canSpawn = isCanMove = HungerSystem.instance.canDecrease = FishController.instance.canMove = false;

                // Optional: stop all fishing actions
                leftHook = null;
                rightHook = null;
                isCasting = false;
            }
        }
    }


}

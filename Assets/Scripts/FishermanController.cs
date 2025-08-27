using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FishermanController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Rod Selection")]
    public Transform leftRod;
    public Transform rightRod;
    private Transform currentRod;

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

    private bool isCasting = false;
    private bool meterIncreasing = true;

    [HideInInspector] public GameObject leftHook = null;
    [HideInInspector] public GameObject rightHook = null;


    void Start()
    {
        currentRod = leftRod;
        if (castingMeter != null)
            castingMeter.value = 0;
    }

    void Update()
    {
        HandleMovement();
        HandleRodSelection();
        HandleCasting();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Here you can check if a hook has a fish attached
            // For now, just print a log
            Debug.Log("Tug-of-war action! Space pressed while fish is hooked.");
        }
    }

    void HandleMovement()
    {
        if (leftHook == null && rightHook == null)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            Vector3 move = new Vector3(moveInput * moveSpeed * Time.deltaTime, 0, 0);
            transform.position += move;

            Vector3 clampedPos = transform.position;
            transform.position = clampedPos;

        }
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
            if ((currentRod == leftRod && leftHook != null) ||
                (currentRod == rightRod && rightHook != null))
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
        isCasting = false;
        StopCoroutine(CastMeterRoutine());

        if (hookPrefab != null && currentRod != null)
        {
            GameObject hook = Instantiate(hookPrefab, currentRod.position, Quaternion.identity);
            hook.name = "Hook";

            Hook hookScript = hook.GetComponent<Hook>();
            if (hookScript != null)
            {
                hookScript.rodTip = currentRod;

                // Automatic worm attach
                hookScript.AttachWorm();

                // Launch hook based on meter value
                float castDistance = castingMeter.value * maxCastDistance;
                hookScript.LaunchDownWithDistance(castDistance);
            }

            // Track hook per rod
            if (currentRod == leftRod) leftHook = hook;
            else rightHook = hook;

            if (worms > 0)
            {
                worms--;
                Debug.Log("Worm used! Remaining: " + worms);
            }
        }

        castingMeter.value = 0;
    }
    public void ClearHookReference(GameObject hook)
    {
        if (hook == leftHook) leftHook = null;
        if (hook == rightHook) rightHook = null;
    }

    // Check worms and print lose message
    public void CheckWorms()
    {
        if (worms <= 0)
        {
            if (GameManager.instance != null && GameManager.instance.gameOverText != null)
            {
                GameManager.instance.ShowGameOver("Fisherman Lose!\nFishes Win!");
            }

            // Optional: stop all fishing actions
            leftHook = null;
            rightHook = null;
            isCasting = false;
        }
    }

}

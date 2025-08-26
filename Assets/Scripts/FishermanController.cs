using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Worms")]
    public int worms = 10;

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
    }

    void HandleMovement()
    {
        // Boat move allowed only if no hook exists
        if (leftHook == null && rightHook == null)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            Vector3 move = new Vector3(moveInput * moveSpeed * Time.deltaTime, 0, 0);
            transform.position += move;
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

        if (isCasting && (!Input.GetKey(castKey1) || !Input.GetKey(castKey2)))
        {
            ReleaseCast();
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

                // ✅ Automatic worm attach
                hookScript.AttachWorm();

                // Launch hook down
                hookScript.LaunchDown();
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
        StartCoroutine(FishInteractionRoutine());
    }

    IEnumerator FishInteractionRoutine()
    {
        Debug.Log("Waiting for Fish Interaction...");
        yield return new WaitForSeconds(2f);

        int randomCase = Random.Range(0, 3);
        switch (randomCase)
        {
            case 0:
                Debug.Log("Case A: Fish bites → Struggle Phase");
                break;
            case 1:
                Debug.Log("Case B: Fish puts Junk → worm wasted");
                break;
            case 2:
                Debug.Log("Case C: No interaction → line returns");
                break;
        }
    }

    // Called by Hook on destroy
    public void ClearHookReference(GameObject hook)
    {
        if (hook == leftHook) leftHook = null;
        if (hook == rightHook) rightHook = null;
    }
}

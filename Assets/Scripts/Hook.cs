using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Hook : MonoBehaviour
{
    public Transform rodTip;
    public LineRenderer lineRenderer;
    public float dropSpeed = 3f;
    public float dropDistance = 10f;

    public GameObject wormPrefab;      // Assign Worm prefab in inspector
    public Transform wormParent;       // Assign WormParent transform in inspector
    private GameObject wormInstance;   // Instance of worm

    private bool hasWorm = false;
    private bool isReturning = false;

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void Update()
    {
        if (rodTip == null || lineRenderer == null)
            return;

        lineRenderer.SetPosition(0, rodTip.position);
        lineRenderer.SetPosition(1, transform.position);

        if (Input.GetKeyDown(KeyCode.Space) && !isReturning)
        {
            StartCoroutine(ReturnToRod());
        }
    }

    // Worm dynamically generate in WormParent
    public void AttachWorm()
    {
        if (wormPrefab != null && !hasWorm && wormParent != null)
        {
            wormInstance = Instantiate(wormPrefab, wormParent.position, Quaternion.identity, wormParent);
            wormInstance.transform.localPosition = Vector3.zero; // center in WormParent
            hasWorm = true;
            Debug.Log("Worm dynamically attached to HookParent");
        }
    }

    public void LaunchDown()
    {
        StartCoroutine(MoveDown(dropDistance));
    }

    private IEnumerator MoveDown(float distance)
    {
        Vector3 target = transform.position + Vector3.down * distance;
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, dropSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator ReturnToRod()
    {
        isReturning = true;
        Vector3 target = rodTip.position;
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, dropSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject); // hook + worm destroyed together
    }

    void OnDestroy()
    {
        FishermanController fc = FindObjectOfType<FishermanController>();
        if (fc != null)
        {
            fc.ClearHookReference(gameObject);
        }
    }
}

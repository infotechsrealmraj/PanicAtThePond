using FishNet;
using FishNet.Object;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Hook : NetworkBehaviour
{
    public Transform rodTip;
    public LineRenderer lineRenderer;
    public float dropSpeed = 3f;

    public GameObject wormPrefab;
    public Transform wormParent;
    private GameObject wormInstance;

    private bool hasWorm = false;
    private bool isReturning = false;

    public float minDistance = 2f;   // Minimum hook drop distance
    public float maxDistance = 15f;  // Maximum hook drop distance

    public static Hook instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

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

        ShowRope();

        if (Input.GetMouseButtonDown(1) && !isReturning) // 1 = right mouse button
        {
            LoadReturnToRod();
        }

    }

    public void ShowRope()
    {
        if (rodTip == null || lineRenderer == null)
            return;

        lineRenderer.SetPosition(0, rodTip.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    public void AttachWorm()
    {
        if (wormPrefab != null && !hasWorm && wormParent != null)
        {
            GameObject worm  = Instantiate(wormPrefab, wormParent.position, Quaternion.identity, wormParent);
            worm.transform.localPosition = Vector3.zero;
            worm.GetComponent<PolygonCollider2D>().enabled = false;
            hasWorm = true;
            wormInstance =  worm;
            Spawn(worm);
        }
    }


    public void LaunchDownWithDistance(float distance)
    {
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        StartCoroutine(MoveDown(distance));
    }

    private IEnumerator MoveDown(float distance)
    {
        Vector3 target = transform.position + Vector3.down * distance;
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, dropSpeed * Time.deltaTime);
            yield return null;
        }

        wormInstance.GetComponent<PolygonCollider2D>().enabled = true;

    }

    public void LoadReturnToRod()
    {
        StartCoroutine(ReturnToRod());
    }

    private IEnumerator ReturnToRod()
    {
        if (wormInstance != null)
            wormInstance.GetComponent<PolygonCollider2D>().enabled = false;

        isReturning = true;
        Vector3 target = rodTip.position;

        // Detach worm from hook so it stays in scene
        if (wormInstance != null)
        {
            wormInstance.transform.parent = null; // worm ko hook se alag kar do
            wormInstance = null; // reference clear
            hasWorm = false;
        }

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, dropSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject); // hook destroy
    }


    // ✅ Updated to avoid obsolete warning
    void OnDestroy()
    {
        FishermanController fc = Object.FindFirstObjectByType<FishermanController>();
        if (fc != null)
        {
            fc.ClearHookReference(this.gameObject);
            fc.CheckWorms();

        }
    }
}

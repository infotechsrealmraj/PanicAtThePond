using UnityEngine;

public class WormAutoParent : MonoBehaviour
{

    public Transform Parent;
    public PolygonCollider2D PolygonCollider2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Parent != null)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            PolygonCollider2D.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

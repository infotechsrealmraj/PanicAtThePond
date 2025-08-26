using UnityEngine;

public class FishController : MonoBehaviour
{
    [Header("Fish Stats")]
    public int hunger = 100; // start with full hunger
    public float speed = 5f;

    public Vector2 minBounds = new Vector2(-8f, -4f);
    public Vector2 maxBounds = new Vector2(8f, 4f);

    public float originalScaleX = 0.4f;
    public float originalScaleY = 0.4f;

    private Rigidbody2D rb;
    private bool canMove = true;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            AutoFishMove();
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(moveX, moveY) * speed;

        if (moveX < 0)
        {
            transform.localScale = new Vector3(originalScaleX, originalScaleY, 1);
        }
        else if (moveX > 0)
        {

            transform.localScale = new Vector3(-originalScaleX, originalScaleY, 1);
        }

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x, maxBounds.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minBounds.y, maxBounds.y);
        transform.position = clampedPos;



    }

    public void EnableMovement(bool state)
    {
        canMove = state;
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Worm"))
        {
            HungerSystem.instance.AddHunger(75f);
            Destroy(other.gameObject);
        }
        if (other.CompareTag("GoldTrout"))
        {
            HungerSystem.instance.AddHunger(25f);
            Destroy(other.gameObject);
        }
    }

    public float Autospeed = 3f;
    internal Vector3 direction = Vector3.left;
    public void AutoFishMove()
    {

        if (transform.position.x > 7.5)
        {
            Debug.Log("Left");
            direction = Vector3.left;

        }
        else if (transform.position.x < -7.5)
        {
            Debug.Log("Right");
            direction = Vector3.right;
        }


        if (direction.x < 0)
        {
            transform.localScale = new Vector3(originalScaleX, originalScaleY, 1);
        }
        else
        {
            transform.localScale = new Vector3(-originalScaleX, originalScaleY, 1);
        }

        transform.position += direction * Autospeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, -2f, transform.position.z);

    }
}

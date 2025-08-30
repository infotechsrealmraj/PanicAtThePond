using FishNet.Object;
using UnityEngine;

public class demoplayer : NetworkBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // ये चेक बहुत जरूरी है! नहीं तो हर client हर player को control करेगा
        if (!IsOwner)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, v, 0) * speed * Time.deltaTime);
    }
}
